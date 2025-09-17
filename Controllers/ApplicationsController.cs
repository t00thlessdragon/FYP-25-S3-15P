using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using FYP_25_S3_15P.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP_25_S3_15P.Controllers
{
    // [Authorize(Roles = "PlatformAdmin")]
    public class ApplicationsController : Controller
    {
        private readonly SmartDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IEmailSender _email;

        public ApplicationsController(
            SmartDbContext db,
            IPasswordHasher<User> hasher,
            IEmailSender email)
        {
            _db = db;
            _hasher = hasher;
            _email = email;
        }

        // POST: /Applications/Approve
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id) => await SetStatus(id, "Approved");

        // POST: /Applications/Reject
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id) => await SetStatus(id, "Rejected");

        // (Optional) POST: /Applications/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var app = await _db.ApplicationForms.FirstOrDefaultAsync(a => a.AppId == id);
            if (app == null) return NotFound();

            _db.ApplicationForms.Remove(app);
            await _db.SaveChangesAsync();

            TempData["Toast"] = "Application deleted.";
            return RedirectToAction("ApplicationMaster", "PADashboard");
        }

        // ----------------- helpers -----------------
        private async Task<IActionResult> SetStatus(int id, string status)
        {
            var app = await _db.ApplicationForms.FirstOrDefaultAsync(a => a.AppId == id);
            if (app == null) return NotFound();

            // We'll email after commit
            bool accountJustCreated = false;
            string emailTo = app.Email;
            string emailName = app.ApplicantName;
            string? generatedPassword = null;

            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                app.Status = status;
                app.UpdatedAt = DateTime.UtcNow;

                if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
                {
                    // Validate university
                    var uniId = app.UniID;
                    var uniExists = await _db.Universities.AnyAsync(u => u.UniID == uniId);
                    if (!uniExists)
                    {
                        TempData["Toast"] = "Cannot approve: application has no valid university.";
                        return RedirectToAction("ApplicationMaster", "PADashboard");
                    }

                    // Check for existing user in that university
                    var existing = await _db.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == app.Email && u.UniID == uniId);

                    if (existing == null)
                    {
                        // Create new Uni Admin + generate password
                        generatedPassword = GeneratePassword(12);

                        var user = new User
                        {
                            Name   = app.ApplicantName,
                            Email  = app.Email,
                            RoleId = 2,          // University Admin
                            UniID  = uniId,
                            Status = "Active",
                            // MustChangePassword = true,
                            // CreatedAt = DateTime.UtcNow
                        };

                        user.Password = _hasher.HashPassword(user, generatedPassword);
                        _db.Users.Add(user);

                        accountJustCreated = true;
                    }
                    else
                    {
                        // No creation. We'll still email to say it's approved.
                        TempData["ToastExtra"] =
                            $"An account already exists for {app.Email} in this university; skipping creation.";
                    }
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                // --- Email after commit ---
                if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
                {
                    var loginUrl = Url.Action("Login", "Account", values: null, protocol: Request.Scheme)
                                   ?? "/Account/Login";

                    try
                    {
                        if (accountJustCreated && generatedPassword is not null)
                        {
                            var html = BuildApprovedEmailWithPassword(emailName, emailTo, generatedPassword, loginUrl);
                            await _email.SendAsync(emailTo, "SMART: Application Approved", html);
                            TempData["ToastExtra"] = $"Account created and email sent to {emailTo}.";
                        }
                        else
                        {
                            var html = BuildApprovedEmailNoPassword(emailName, loginUrl);
                            await _email.SendAsync(emailTo, "SMART: Application Approved", html);
                            TempData["ToastExtra"] = $"Approval email sent to {emailTo}.";
                        }
                    }
                    catch (Exception mailEx)
                    {
                        // Don't block the UX if SMTP fails
                        TempData["ToastExtra"] = $"Approved, but email could not be sent: {mailEx.Message}";
                    }
                }

                TempData["Toast"] = $"Application {status}.";
                return RedirectToAction("ApplicationMaster", "PADashboard");
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                TempData["Toast"] = "Failed to update: database constraint error.";
                TempData["ToastExtra"] = ex.InnerException?.Message ?? ex.Message;
                return RedirectToAction("ApplicationMaster", "PADashboard");
            }
        }

        /// <summary>Generates a random password with A–Z, a–z, and digits.</summary>
        private static string GeneratePassword(int length)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(alphabet[bytes[i] % alphabet.Length]);
            return sb.ToString();
        }

        private static string BuildApprovedEmailWithPassword(string name, string email, string password, string loginUrl)
        {
            string H(string s) => System.Net.WebUtility.HtmlEncode(s);
            return $@"
<p>Dear {H(name)},</p>
<p>Your application has been approved. An account has been created for you automatically. Here are your login details:</p>
<ul>
  <li><strong>Email:</strong> {H(email)}</li>
  <li><strong>Password:</strong> {H(password)}</li>
  <li><strong>Login URL:</strong> <a href=""{loginUrl}"">{loginUrl}</a></li>
</ul>
<p>Thank you!</p>
<p>Regards,<br/>SMART Team</p>";
        }

        private static string BuildApprovedEmailNoPassword(string name, string loginUrl)
        {
            string H(string s) => System.Net.WebUtility.HtmlEncode(s);
            return $@"
<p>Dear {H(name)},</p>
<p>Your application has been approved.</p>
<p>You can now sign in here: <a href=""{loginUrl}"">{loginUrl}</a>.</p>
<p>If you’ve forgotten your password, please use the “Forgot Password” link on the sign-in page.</p>
<p>Thank you!</p>
<p>Regards,<br/>SMART Team</p>";
        }
    }
}
