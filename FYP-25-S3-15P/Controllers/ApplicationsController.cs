using System;
using System.Threading.Tasks;
using FYP_25_S3_15P.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP_25_S3_15P.Controllers
{
    // [Authorize(Roles = "PlatformAdmin")]
    public class ApplicationsController : Controller
    {
        private readonly SmartDbContext _db;
        public ApplicationsController(SmartDbContext db) => _db = db;

        // POST: /Applications/Approve
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
            => await SetStatus(id, "Approved");

        // POST: /Applications/Reject
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
            => await SetStatus(id, "Rejected");

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

            app.Status = status;
            app.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Toast"] = $"Application {status}.";
            return RedirectToAction("ApplicationMaster", "PADashboard");
        }
    }
}