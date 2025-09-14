using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;            // User, ApplicationForm, University
using Microsoft.AspNetCore.Identity;   // IPasswordHasher<T>
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP_25_S3_15P.Controllers
{
    // [Authorize(Roles = "PlatformAdmin")]
    public class ApplicationsController : Controller
    {
        private readonly SmartDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public ApplicationsController(SmartDbContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
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

            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                app.Status = status;
                app.UpdatedAt = DateTime.UtcNow;

                if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
                {
                    // Ensure the application is linked to a valid university
                    var uniId = app.UniID;
                    var uniExists = await _db.Universities.AnyAsync(u => u.UniID == uniId);
                    if (!uniExists)
                    {
                        TempData["Toast"] = "Cannot approve: application has no valid university.";
                        return RedirectToAction("ApplicationMaster", "PADashboard");
                    }

                    // Avoid duplicate account in the same university
                    var existing = await _db.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == app.Email && u.UniID == uniId);

                    if (existing == null)
                    {
                        var plainPassword = GeneratePassword(12);

                        var user = new User
                        {
                            Name   = app.ApplicantName,
                            Email  = app.Email,
                            RoleId = 2,          // University Admin
                            UniID  = uniId,
                            Status = "Active",
                            // If your model has these, they will map; otherwise ignore:
                            // MustChangePassword = true,
                            // CreatedAt = DateTime.UtcNow
                        };

                        user.Password = _hasher.HashPassword(user, plainPassword);

                        _db.Users.Add(user);

                        TempData["ToastExtra"] =
                            $"Created University Admin for {app.Email}. Temp password: {plainPassword}";
                    }
                    else
                    {
                        TempData["ToastExtra"] =
                            $"An account already exists for {app.Email} in this university; skipped duplicate.";
                    }
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Toast"] = $"Application {status}.";
                return RedirectToAction("ApplicationMaster", "PADashboard");
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();

                // Common causes: FK (missing UniID), unique index collisions
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
    }
}
