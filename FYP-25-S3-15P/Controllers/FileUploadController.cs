using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using FYP_25_S3_15P.Services;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace FYP_25_S3_15P.Controllers
{
    [Authorize(Roles = "University Admin")]
    public class FileUploadController : Controller
    {
        private readonly SmartDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IEmailSender _email;
        private readonly ILogger<FileUploadController> _logger;

        private const string TestEmailRedirect = "waiyan9600@gmail.com";

        public FileUploadController(SmartDbContext db, IPasswordHasher<User> hasher, IEmailSender email, ILogger<FileUploadController> logger)
        {
            _db = db;
            _hasher = hasher;
            _email = email;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

[HttpPost]
public async Task<IActionResult> UploadStaff(IFormFile staffFile)
{
    try
    {
        if (staffFile == null || staffFile.Length == 0)
            return Json(new { success = false, message = "Please select a valid CSV file." });

        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        var admin = await _db.Users.Include(u => u.University)
                                   .FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (admin == null || admin.UniID == null)
            return Json(new { success = false, message = "You are not linked to a valid university." });

        int uniId = admin.UniID.Value;
        int successCount = 0, skipCount = 0, lineNo = 0;

        // Cache roles and modules
        var rolesDict = await _db.Roles.ToDictionaryAsync(r => r.Name.ToUpperInvariant(), r => r);
        var modulesDict = await _db.Modules.ToDictionaryAsync(m => m.ModuleCode.ToUpperInvariant(), m => m);

        using var reader = new StreamReader(staffFile.OpenReadStream());
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            lineNo++;
            if (lineNo == 1 && line.Contains("StaffID", StringComparison.OrdinalIgnoreCase))
                continue; // skip header

            var cols = line.Split(',');
            if (cols.Length < 7)
            {
                skipCount++;
                continue;
            }

            string staffId = cols[0].Trim().Trim('"');
            string name = cols[1].Trim();
            string email = cols[2].Trim().ToLowerInvariant();
            string roleName = cols[3].Trim();
            string uniNameCsv = cols[4].Trim();
            string programName = cols[5].Trim();
            string modulesRaw = cols[6].Trim();

            if (string.IsNullOrEmpty(staffId) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                skipCount++;
                continue;
            }

            // Verify University match
            if (!string.IsNullOrWhiteSpace(uniNameCsv))
            {
                var adminUniName = admin.University?.UniName ?? "";
                if (!string.Equals(adminUniName.Trim(), uniNameCsv.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    skipCount++;
                    continue;
                }
            }

            // Verify Role
            if (!rolesDict.TryGetValue(roleName.ToUpperInvariant(), out var roleEntity))
            {
                skipCount++;
                continue;
            }

            // Verify Program
            UniversityProgram? programEntity = null;
            if (!string.IsNullOrWhiteSpace(programName))
            {
                programEntity = await _db.UniversityPrograms
                    .FirstOrDefaultAsync(p => p.UniID == uniId && p.ProgramName.ToUpper() == programName.ToUpper());
                if (programEntity == null)
                {
                    skipCount++;
                    continue;
                }
            }

            // Create or update User
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.UniID == uniId);
            User userEntity;
            string tempPassword = null;

            if (existingUser == null)
            {
                tempPassword = GenerateTempPassword(12);
                userEntity = new User
                {
                    Name = name,
                    Email = email,
                    UniID = uniId,
                    Status = "Active",
                    MustChangePassword = true,
                    CreatedBy = admin.Id,
                    CreatedAt = DateTime.UtcNow,
                    RoleId = roleEntity.RoleId
                };
                userEntity.Password = _hasher.HashPassword(userEntity, tempPassword);
                _db.Users.Add(userEntity);
                await _db.SaveChangesAsync();
            }
            else
            {
                userEntity = existingUser;
                if (existingUser.RoleId != roleEntity.RoleId)
                {
                    existingUser.RoleId = roleEntity.RoleId;
                    _db.Users.Update(existingUser);
                    await _db.SaveChangesAsync();
                }
            }

            // Ensure StaffProfile
            var staffProfile = await _db.StaffProfiles.FirstOrDefaultAsync(s => s.StaffID == staffId);
            if (staffProfile == null)
            {
                staffProfile = new StaffProfile
                {
                    StaffID = staffId,
                    UserID = userEntity.Id,
                    CreatedBy = admin.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _db.StaffProfiles.Add(staffProfile);
                await _db.SaveChangesAsync();
            }
            else
            {
                staffProfile.UserID = userEntity.Id;
                staffProfile.UpdatedAt = DateTime.UtcNow;
                staffProfile.UpdatedBy = admin.Id;
                _db.StaffProfiles.Update(staffProfile);
                await _db.SaveChangesAsync();
            }

            // Handle Modules (comma, semicolon, pipe separated)
            var moduleCodes = modulesRaw
                .Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim().Trim('"').ToUpperInvariant())
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct();

            foreach (var modCode in moduleCodes)
            {
                if (!modulesDict.TryGetValue(modCode, out var module))
                    continue;

                bool linked = await _db.StaffModules.AnyAsync(sm => sm.StaffID == staffProfile.ID && sm.ModuleID == module.ID);
                if (!linked)
                {
                    _db.StaffModules.Add(new StaffModules
                    {
                        StaffID = staffProfile.ID,
                        ModuleID = module.ID,
                        CreatedBy = admin.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _db.SaveChangesAsync();

            successCount++;
        }

        return Json(new
        {
            success = true,
            message = $"✅ {successCount} staff processed. ⏭️ {skipCount} rows skipped."
        });
    }
    catch (Exception ex)
    {
        // Log the exception and prevent saving if there's an error
        _logger.LogError(ex, "Error occurred during the upload process.");
        return Json(new { success = false, message = "An error occurred while uploading the file. Please try again." });
    }
}


        private static string GenerateTempPassword(int length)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = alphabet[bytes[i] % alphabet.Length];
            return new string(chars);
        }
    }
}
