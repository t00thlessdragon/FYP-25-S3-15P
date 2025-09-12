using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Controllers;

public class SubscriptionPlansController : Controller
{
    private readonly SmartDbContext _db;
    public SubscriptionPlansController(SmartDbContext db) => _db = db;

    // PUBLIC pricing page
    public async Task<IActionResult> Index()
    {
        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .Include(p => p.PlanFeatures)
                .ThenInclude(pf => pf.Feature)
            .OrderBy(p => p.Price)
            .ToListAsync();

        return View(plans); // Views/SubscriptionPlans/Index.cshtml
    }

    // ===== CRUD used by the dashboard (Plans tab) =====
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Price,Description")] SubscriptionPlan plan)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("Index", "PADashboard", new { tab = "plans" });

        _db.Add(plan);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "PADashboard", new { tab = "plans" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("PlanID,Name,Price,Description")] SubscriptionPlan plan)
    {
        if (id != plan.PlanID) return NotFound();
        if (!ModelState.IsValid)
            return RedirectToAction("Index", "PADashboard", new { tab = "plans" });

        _db.Update(plan);
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
