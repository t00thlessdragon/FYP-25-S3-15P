using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Controllers
{
    [Authorize(Roles = "Platform Admin")]
    public class FAQController : Controller
    {
        private readonly SmartDbContext _db;
        public FAQController(SmartDbContext db) => _db = db;

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Question,Answer,SortOrder,IsActive")] FAQ item)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "PADashboard", new { tab = "faq" });

            _db.FAQs.Add(item);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "PADashboard", new { tab = "faq" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Question,Answer,SortOrder,IsActive")] FAQ item)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "PADashboard", new { tab = "faq" });

            var existing = await _db.FAQs.FindAsync(item.Id);
            if (existing == null) return NotFound();

            existing.Question   = item.Question?.Trim() ?? "";
            existing.Answer     = item.Answer?.Trim() ?? "";
            existing.SortOrder  = item.SortOrder;
            existing.IsActive   = item.IsActive;
            existing.UpdatedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "PADashboard", new { tab = "faq" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var row = await _db.FAQs.FindAsync(id);
            if (row != null)
            {
                _db.FAQs.Remove(row);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index", "PADashboard", new { tab = "faq" });
        }
    }
}
