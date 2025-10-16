using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models; // UniStaffVm, StaffProfile, UniversityProgram, Module, StaffModules, User, Role, etc.
using FYP_25_S3_15P.Services; // IEmailSender
using Microsoft.AspNetCore.Identity;

namespace FYP_25_S3_15P.Controllers
{
    [Authorize(Roles = "University Admin")]
    public class UADashboardController : Controller
    {
        private readonly SmartDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IEmailSender _email;

        // When testing, redirect all outgoing email to this address:
        private const string TestEmailRedirect = "waiyan9600@gmail.com";

        public UADashboardController(SmartDbContext db, IPasswordHasher<User> hasher, IEmailSender email)
        {
            _db = db;
            _hasher = hasher;
            _email = email;
        }

        // University Admin Home
        [HttpGet("/UADashboard")]
        public async Task<IActionResult> Index() => await UniStaff();

        // -------------------------------
        // University Staff Table
        // -------------------------------
        [HttpGet("/UADashboard/UniStaff")]
        public async Task<IActionResult> UniStaff()
        {
            var currentEmail = User.FindFirstValue(ClaimTypes.Email);
            var admin = await _db.Users
                .Include(u => u.University)
                .FirstOrDefaultAsync(u => u.Email == currentEmail);

            if (admin == null || admin.UniID == null)
            {
                TempData["Error"] = "Your account is not linked to any university.";
                return RedirectToAction("Index", "Home");
            }

            int uniId = admin.UniID.Value;

            // Load staff profiles
            var staffQuery = _db.StaffProfiles
                .Include(s => s.User) // Ensure User data (like Email) is included
                .Where(s => s.User != null && s.User.UniID == uniId)
                .OrderBy(s => s.User!.Name);

            var staffList = await staffQuery
                .Select(s => new UniStaffVm.Row
                {
                    Id = s.ID,
                    StaffID = s.StaffID,
                    UserID = s.UserID,
                    Name = s.User != null ? s.User.Name : "-",
                    Email = s.User != null ? s.User.Email : "-",  // Ensure Email is included
                    UniName = s.User != null && s.User.UniID != null
                        ? _db.Universities.Where(x => x.UniID == s.User.UniID.Value).Select(x => x.UniName).FirstOrDefault()
                        : "-",
                    Status = s.User != null ? (s.User.IsLocked ? "Locked" : (s.User.Status ?? "Active")) : "-",
                    IsLocked = s.User != null ? s.User.IsLocked : false,
                    LastLogin = s.User != null ? s.User.LastLogin : null,
                    AssignedModules = _db.StaffModules
                        .Where(sm => sm.StaffID == s.ID)
                        .Select(sm => new UniStaffVm.ModuleRow
                        {
                            ID = sm.ID,
                            ModuleID = sm.ModuleID ?? 0,
                            ModuleCode = sm.Module != null ? sm.Module.ModuleCode : "",
                            ModuleName = sm.Module != null ? sm.Module.ModuleName : ""
                        }).ToList()
                }).ToListAsync();

            var vm = new UniStaffVm { Staff = staffList };
            return View("~/Views/Dashboards/UA/UserMaster.cshtml", vm);
        }

        // -------------------------------
        // Toggle Active
        // -------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, bool value)
        {
            var staff = await _db.StaffProfiles.Include(s => s.User).FirstOrDefaultAsync(s => s.ID == id);
            if (staff?.User == null)
            {
                TempData["Error"] = "Staff not found.";
                return RedirectToAction(nameof(UniStaff));
            }

            var admin = await _db.Users.FirstOrDefaultAsync(u => u.Email == User.FindFirstValue(ClaimTypes.Email));
            if (admin?.UniID != staff.User.UniID)
            {
                TempData["Error"] = "You cannot modify staff from another university.";
                return RedirectToAction(nameof(UniStaff));
            }

            staff.User.IsLocked = !value;
            staff.UpdatedAt = DateTime.UtcNow;
            staff.User.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Flash"] = value ? "Staff activated." : "Staff deactivated.";
            return RedirectToAction(nameof(UniStaff));
        }

        // -------------------------------
        // Reset Password
        // -------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var staff = await _db.StaffProfiles.Include(s => s.User).FirstOrDefaultAsync(s => s.ID == id);
            if (staff?.User == null)
            {
                TempData["Error"] = "Staff not found.";
                return RedirectToAction(nameof(UniStaff));
            }

            var admin = await _db.Users.FirstOrDefaultAsync(u => u.Email == User.FindFirstValue(ClaimTypes.Email));
            if (admin?.UniID != staff.User.UniID)
            {
                TempData["Error"] = "You cannot reset passwords for staff outside your university.";
                return RedirectToAction(nameof(UniStaff));
            }

            var tempPassword = GenerateTempPassword(12);
            staff.User.Password = _hasher.HashPassword(staff.User, tempPassword);
            staff.User.MustChangePassword = true;
            staff.User.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            try
            {
                string H(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");
                var subject = "SMART: Password Reset by University Admin";
                var body = $@"<p>Dear {H(staff.User.Name)},</p>
<p>Your password has been reset by your University Admin. Please use this new password to login: <strong>{H(tempPassword)}</strong>.</p>
<p>Regards,<br/>SMART Team</p>";

                await _email.SendAsync(TestEmailRedirect, subject, body);
                TempData["Flash"] = $"Temporary password emailed to {TestEmailRedirect}.";
            }
            catch
            {
                TempData["Flash"] = "Password was reset but sending the email failed.";
            }

            return RedirectToAction(nameof(UniStaff));
        }

        // -------------------------------
        // Helpers
        // -------------------------------
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
