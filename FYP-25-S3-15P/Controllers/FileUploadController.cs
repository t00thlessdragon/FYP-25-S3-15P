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
using Microsoft.Extensions.Logging; 

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

        // Cache lookup tables
        var rolesDict = await _db.Roles.ToDictionaryAsync(r => r.Name.ToUpperInvariant(), r => r);
        var modulesDict = await _db.Modules.ToDictionaryAsync(m => m.ModuleCode.ToUpperInvariant(), m => m);
        
        // Cache Session names to IDs.
        var sessionsDict = await _db.Sessions.ToDictionaryAsync(s => s.SessionName.ToUpperInvariant().Trim(), s => s.SessionID);

        using var reader = new StreamReader(staffFile.OpenReadStream());
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            lineNo++;
            if (lineNo == 1 && line.Contains("StaffID", StringComparison.OrdinalIgnoreCase))
                continue; // skip header

            var cols = line.Split(',');
            // CSV now has 8 columns (index 0 to 7)
            if (cols.Length < 8)
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
            string sessionNameCsv = cols[7].Trim(); 

            if (string.IsNullOrEmpty(staffId) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                skipCount++;
                continue;
            }

            // University verification
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
            
            // Get Session ID 
            int? sessionIdToAssign = null;
            if (!string.IsNullOrWhiteSpace(sessionNameCsv))
            {
                string lookupKey = sessionNameCsv.Trim().Trim('"').ToUpperInvariant(); 
                if (sessionsDict.TryGetValue(lookupKey, out var matchedSessionId))
                {
                    sessionIdToAssign = matchedSessionId;
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
                // TODO: Email the staff the tempPassword
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
                    CreatedAt = DateTime.UtcNow,
                    CurrentSessionID = sessionIdToAssign // Assign Session ID on creation
                };
                _db.StaffProfiles.Add(staffProfile);
                await _db.SaveChangesAsync();
            }
            else
            {
                staffProfile.UserID = userEntity.Id;
                staffProfile.UpdatedAt = DateTime.UtcNow;
                staffProfile.UpdatedBy = admin.Id;
                staffProfile.CurrentSessionID = sessionIdToAssign; // Update Session ID
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
        _logger.LogError(ex, "Error occurred during the upload process.");
        return Json(new { success = false, message = "An error occurred while uploading the file. Please try again." });
    }
}

[HttpPost]
public async Task<IActionResult> UploadStudent(IFormFile studentFile)
{
    try
    {
        if (studentFile == null || studentFile.Length == 0)
            return Json(new { success = false, message = "Please select a valid CSV file." });

        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        var admin = await _db.Users.Include(u => u.University)
                                   .FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (admin == null || admin.UniID == null)
            return Json(new { success = false, message = "You are not linked to a valid university." });

        int uniId = admin.UniID.Value;
        int successCount = 0, skipCount = 0, lineNo = 0;

        // Cache lookup tables
        var modulesDict = await _db.Modules.ToDictionaryAsync(m => m.ModuleCode.ToUpperInvariant(), m => m);
        var sessionsDict = await _db.Sessions.ToDictionaryAsync(s => s.SessionName.ToUpperInvariant().Trim(), s => s.SessionID);

        using var reader = new StreamReader(studentFile.OpenReadStream());
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            lineNo++;
            // Skip header (adjust header check if your header is different)
            if (lineNo == 1 && line.Contains("StudentID", StringComparison.OrdinalIgnoreCase))
                continue;

            var cols = line.Split(',');
            // CSV fields (9 columns): StudentID(0), Name(1), Email(2), University(3), Program(4), Course(5), Year of Study(6), Session(7), Modules(8)
            if (cols.Length < 9)
            {
                _logger.LogWarning($"Skipped line {lineNo} due to missing columns: {line}");
                skipCount++;
                continue;
            }

            string studentId = cols[0].Trim().Trim('"');
            string name = cols[1].Trim();
            string email = cols[2].Trim().ToLowerInvariant();
            string uniNameCsv = cols[3].Trim();
            string programName = cols[4].Trim();
            string courseName = cols[5].Trim();
            string yearOfStudyRaw = cols[6].Trim();
            string sessionNameCsv = cols[7].Trim(); 
            string modulesRaw = cols[8].Trim(); 

            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                skipCount++;
                continue;
            }

            // --- 1. University Verification ---
            if (!string.IsNullOrWhiteSpace(uniNameCsv))
            {
                var adminUniName = admin.University?.UniName ?? "";
                if (!string.Equals(adminUniName.Trim(), uniNameCsv.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    skipCount++;
                    continue;
                }
            }

            // --- 2. Program Lookup ---
            var programEntity = await _db.UniversityPrograms
                .FirstOrDefaultAsync(p => p.UniID == uniId && p.ProgramName.ToUpper() == programName.ToUpper().Trim());
            if (programEntity == null)
            {
                skipCount++;
                continue;
            }

            // --- 3. Course Lookup ---
            var courseEntity = await _db.Courses
                .FirstOrDefaultAsync(c => c.ProgramID == programEntity.ID && c.CourseName.ToUpper() == courseName.ToUpper().Trim());
            if (courseEntity == null)
            {
                skipCount++;
                continue;
            }
            
            // --- 4. Session Lookup ---
            int? sessionIdToAssign = null;
            if (!string.IsNullOrWhiteSpace(sessionNameCsv))
            {
                string lookupKey = sessionNameCsv.Trim().Trim('"').ToUpperInvariant(); 
                if (sessionsDict.TryGetValue(lookupKey, out var matchedSessionId))
                {
                    sessionIdToAssign = matchedSessionId;
                }
            }

            // --- 5. Year of Study Parsing ---
            int? yearOfStudyInt = null;
            if (int.TryParse(yearOfStudyRaw, out int parsedYear))
            {
                yearOfStudyInt = parsedYear;
            }
            else if (yearOfStudyRaw.Contains("Year", StringComparison.OrdinalIgnoreCase))
            {
                 // Attempt to parse "Year X"
                 string num = new string(yearOfStudyRaw.Where(char.IsDigit).ToArray());
                 if (int.TryParse(num, out int parsedYearFromStr))
                 {
                    yearOfStudyInt = parsedYearFromStr;
                 }
            }


            // --- 6. Create or update User ---
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.UniID == uniId);
            User userEntity;
            string tempPassword = null;
            // IMPORTANT: Define the actual Student Role ID (e.g., 3)
            const int StudentRoleId = 3; 

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
                    RoleId = StudentRoleId 
                };
                userEntity.Password = _hasher.HashPassword(userEntity, tempPassword);
                _db.Users.Add(userEntity);
                await _db.SaveChangesAsync();
                // TODO: Email the student the tempPassword
            }
            else
            {
                userEntity = existingUser;
                // Ensure existing user has the correct role
                if (existingUser.RoleId != StudentRoleId)
                {
                    existingUser.RoleId = StudentRoleId;
                    _db.Users.Update(existingUser);
                    await _db.SaveChangesAsync();
                }
            }


            // --- 7. Ensure StudentProfile ---
            var studentProfile = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.StudentID == studentId);
            if (studentProfile == null)
            {
                studentProfile = new StudentProfile
                {
                    StudentID = studentId,
                    UserID = userEntity.Id,
                    CourseID = courseEntity.ID,
                    SessionID = sessionIdToAssign,
                    YearOfStudy = yearOfStudyInt,
                };
                _db.StudentProfiles.Add(studentProfile);
            }
            else
            {
                // Update existing profile assignments
                studentProfile.UserID = userEntity.Id;
                studentProfile.CourseID = courseEntity.ID;
                studentProfile.SessionID = sessionIdToAssign;
                studentProfile.YearOfStudy = yearOfStudyInt;
                _db.StudentProfiles.Update(studentProfile);
            }
            await _db.SaveChangesAsync();


            // --- 8. Handle Modules (Assignment to StudentModules) ---
            // Clear existing modules first to handle re-enrollment, then re-add based on CSV
            var existingModules = await _db.StudentModules.Where(sm => sm.StudentID == studentProfile.ID).ToListAsync();
            _db.StudentModules.RemoveRange(existingModules);
            await _db.SaveChangesAsync(); // Commit removal

            var moduleCodes = modulesRaw
                .Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim().Trim('"').ToUpperInvariant())
                .Where(m => !string.IsNullOrEmpty(m))
                .Distinct();

            foreach (var modCode in moduleCodes)
            {
                if (!modulesDict.TryGetValue(modCode, out var module))
                    continue;

                // Add the new module linkage
                _db.StudentModules.Add(new StudentModules
                {
                    StudentID = studentProfile.ID,
                    ModuleID = module.ID,
                    CreatedBy = admin.Id, // Assuming admin is the creator
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();

            successCount++;
        }

        return Json(new
        {
            success = true,
            message = $"✅ {successCount} students processed. ⏭️ {skipCount} rows skipped."
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred during student file upload process.");
        return Json(new { success = false, message = "An error occurred while uploading the student file. Please try again." });
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
