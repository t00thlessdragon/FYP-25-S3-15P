using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Controllers;

public class SubscriptionPlansController : Controller
{
    private readonly SmartDbContext _db;
    public SubscriptionPlansController(SmartDbContext db) => _db = db;

    // PUBLIC pricing page (unchanged)
    public async Task<IActionResult> Index()
    {
        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .Include(p => p.PlanFeatures).ThenInclude(pf => pf.Feature)
            .OrderBy(p => p.Price)
            .ToListAsync();

        return View(plans);
    }

    // ===== CRUD used by the dashboard (Plans tab) =====

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Price,Description")] SubscriptionPlan plan)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("Index", "PADashboard", new { tab = "plans" });

        _db.SubscriptionPlans.Add(plan);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "PADashboard", new { tab = "plans" });
    }

    // NOTE: no separate id param anymore (prevents 404 when id isn't posted)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind("PlanID,Name,Price,Description")] SubscriptionPlan plan)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("Index", "PADashboard", new { tab = "plans" });

        var existing = await _db.SubscriptionPlans.FindAsync(plan.PlanID);
        if (existing == null) return NotFound();

        existing.Name        = plan.Name?.Trim() ?? "";
        existing.Price       = plan.Price;
        existing.Description = plan.Description?.Trim();

        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "PADashboard", new { tab = "plans" });
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var plan = await _db.SubscriptionPlans.FindAsync(id);
        if (plan != null)
        {
            _db.SubscriptionPlans.Remove(plan);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index", "PADashboard", new { tab = "plans" });
    }
}
