using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Controllers;

public class SubscriptionPlansController : Controller
{
    private readonly SmartDbContext _db;
    public SubscriptionPlansController(SmartDbContext db) => _db = db;

    // PUBLIC pricing page unchanged
    public async Task<IActionResult> Index()
    {
        var plans = await _db.SubscriptionPlans
            .Include(p => p.Features)
            .AsNoTracking()
            .OrderBy(p => p.Price)
            .ToListAsync();

        return View(plans); // Views/SubscriptionPlans/Index.cshtml
    }

    // CRUD used by the dashboard
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Price,Description,FeaturesText")] SubscriptionPlan plan)
    {
        if (!ModelState.IsValid) return RedirectToAction("Index", "PADashboard", new { tab = "plans" });

        _db.Add(plan);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "PADashboard", new { tab = "plans" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("PlanID,Name,Price,Description,FeaturesText")] SubscriptionPlan plan)
    {
        if (id != plan.PlanID) return NotFound();
        if (!ModelState.IsValid) return RedirectToAction("Index", "PADashboard", new { tab = "plans" });

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

    // When validation fails, re-render the PA dashboard with Plans tab open and the modal reopened
    private async Task<IActionResult> ReRenderPADashboardWithErrors(SubscriptionPlan plan, string activeModal)
    {
        ViewBag.ActiveModal = activeModal;
        ViewBag.ValidationModel = plan;

        var model = new ContentMaster
        {
            Plans = await _db.SubscriptionPlans.AsNoTracking().OrderBy(p => p.Price).ToListAsync(),
            ActiveTab = "plans"
        };

        return View("~/Views/Dashboards/PA/ContentMaster.cshtml", model);
    }
}
