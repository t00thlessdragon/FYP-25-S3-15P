using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using FYP_25_S3_15P.Constants;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;        // ContentMaster, Feature, SubscriptionPlan, User
using FYP_25_S3_15P.ViewModels;    // ApplicationMasterVm, UserMasterVm
using FYP_25_S3_15P.Services;      // IEmailSender
using Microsoft.AspNetCore.Identity; // IPasswordHasher<User>

namespace FYP_25_S3_15P.Controllers
{
    // ✅ Only Platform Admins can access anything in this controller
    [Authorize(Roles = RoleConstants.SubjectCoordinator)]
    public class SCDashboardController : Controller
    {
        private readonly SmartDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IEmailSender _email;

        public SCDashboardController(
            SmartDbContext db,
            IPasswordHasher<User> hasher,
            IEmailSender email)
        {
            _db     = db;
            _hasher = hasher;
            _email  = email;
        }

        // Conventional route for links/redirects
        [HttpGet]
        public Task<IActionResult> Index([FromQuery] string? tab) => ContentMaster(tab);

        // Content dashboard (reads ?tab=)
        [HttpGet("/SCDashboard")]
        public async Task<IActionResult> ContentMaster([FromQuery] string? tab)
        {
            var features = await _db.Features
                .Include(f => f.PlanFeatures).ThenInclude(pf => pf.Plan)
                .OrderBy(f => f.Name)
                .ToListAsync();

            var plans = await _db.SubscriptionPlans
                .OrderBy(p => p.Price)
                .ToListAsync();

            var faqs = await _db.FAQs
                .OrderBy(f => f.SortOrder).ThenBy(f => f.Id)
                .ToListAsync();

            var model = new ContentMaster
            {
                Features  = features,
                Plans     = plans,
                FAQs      = faqs,
                ActiveTab = string.IsNullOrWhiteSpace(tab) ? "features" : tab.ToLowerInvariant()
            };

            return View("~/Views/Dashboards/SC/ContentMaster.cshtml", model);
        }
        // Applications dashboard
        [HttpGet("/SCDashboard/FYPTemplateMaster")]
        public async Task<IActionResult> FYPTemplateMaster([FromQuery] string? tab)
        {
            var activeTab = string.IsNullOrWhiteSpace(tab) ? "templates" : tab.ToLowerInvariant();

            var templates = await _db.FYPTemplates
                .Include(f => f.University)
                .Include(f => f.Programs)
                .OrderBy(f => f.ProjectName)
                .ToListAsync();

            var programs = await _db.Programs
                .OrderBy(p => p.ProgramName)
                .ToListAsync();
            
            var vm = new FYPTemplateMaster {
                FYPTemplates = templates,
                Programs = programs,
                ActiveTab = activeTab
            };
            return View("~/Views/Dashboards/SC/FYPTemplateMaster.cshtml", vm);
        }
        
        // Applications dashboard
        [HttpGet("/SCDashboard/ApplicationMaster")]
        public async Task<IActionResult> ApplicationMaster()
        {
            var rows = await (
                from a in _db.ApplicationForms
                join u in _db.University      on a.UniID  equals u.ID
                join p in _db.SubscriptionPlans on a.PlanID equals p.PlanID
                orderby a.CreatedAt descending
                select new ApplicationMasterVm.Row
                {
                    AppId         = a.AppId,
                    ApplicantName = a.ApplicantName,
                    Email         = a.Email,
                    Role          = a.Role,
                    UniName       = u.UniName,
                    UEN           = u.UniID,
                    PlanId        = p.PlanID,
                    PlanName      = p.Name,
                    Status        = a.Status,
                    CreatedAt     = a.CreatedAt
                }
            ).ToListAsync();

            var vm = new ApplicationMasterVm { Applications = rows };
            return View("~/Views/Dashboards/SC/ApplicationMaster.cshtml", vm);
        }

        // Users dashboard
        [HttpGet("/SCDashboard/UserMaster")]
        public async Task<IActionResult> UserMaster()
        {
            var rows = await (
        from usr in _db.Users

        // join to University (optional)
        join uni in _db.University on usr.UniID equals uni.ID into ug
        from uni in ug.DefaultIfEmpty()

        // join to UserRole to get roles
        join ur in _db.UserRoles on usr.Id equals ur.UserID into urg
        from ur in urg.DefaultIfEmpty()

        // join to Role through UserRole
        join role in _db.Roles on ur.RoleID equals role.Id into rg
        from role in rg.DefaultIfEmpty()

        select new
        {
            usr,
            uni,
            role
        }
    )
    .GroupBy(x => new
    {
        x.usr.Id,
        x.usr.Name,
        x.usr.Email,
        x.usr.UniID,
        x.usr.Status,
        x.usr.IsLocked,
        x.usr.LastLogin,
        UniName = x.uni != null ? x.uni.UniName : "-"
    })
    .Select(g => new UserMasterVm.Row
    {
        Id        = g.Key.Id,
        Name      = g.Key.Name,
        Email     = g.Key.Email,
        UniName   = g.Key.UniName,
        // If user has multiple roles, join them with commas
        Role      = g.Where(x => x.role != null)
                     .Select(x => x.role.Name)
                     .Distinct()
                     .DefaultIfEmpty("-")
                     .Aggregate((r1, r2) => $"{r1}, {r2}"),
        Status    = g.Key.IsLocked ? "Locked" : (g.Key.Status ?? "Active"),
        IsLocked  = g.Key.IsLocked,
        LastLogin = g.Key.LastLogin
    })
    .OrderBy(r => r.Name)
    .ToListAsync();

    var vm = new UserMasterVm { Users = rows };
    return View("~/Views/Dashboards/SC/UserMaster.cshtml", vm);

        }

        // Toggle "Active?" (Active = !IsLocked)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, bool value)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(UserMaster));
            }

            user.IsLocked  = !value;                // true => lock, false => unlock
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Flash"] = value ? "User activated." : "User deactivated.";
            return RedirectToAction(nameof(UserMaster));
        }

        // Reset password: generate a temp password, store it, and email the user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(UserMaster));
            }

            var tempPassword = GenerateTempPassword(12);

            // If your login compares plain text (current AccountController), keep this:
            user.Password = tempPassword;

            // If/when you switch to hashed login, use this instead:
            // user.Password = _hasher.HashPassword(user, tempPassword);

            user.MustChangePassword = true;
            user.UpdatedAt          = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            try
            {
                string H(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");
                var subject = "SMART: Password Reset";
                var body = $@"
<p>Dear {H(string.IsNullOrWhiteSpace(user.Name) ? "" : user.Name)},</p>
<p>Your password has been reset by the Platform Admin. Please use this new password to login <strong>{H(tempPassword)}</strong>.</p>
<p>Thank you!</p>
<p>Regards,<br/>SMART Team</p>";

                await _email.SendAsync(user.Email, subject, body);
                TempData["Flash"] = $"Temporary password emailed to {user.Email}.";
            }
            catch (Exception)
            {
                TempData["Flash"] = "Password was reset but sending the email failed. Please verify SMTP settings.";
            }

            return RedirectToAction(nameof(UserMaster));
        }

        // --- helpers ---
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
