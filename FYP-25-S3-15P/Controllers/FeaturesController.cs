using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Controllers;

public class FeaturesController : Controller
{
    private readonly SmartDbContext _db;
    private readonly IWebHostEnvironment _env;

    public FeaturesController(SmartDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // POST /Features/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, List<int>? selectedPlanIds)
    {
        var f = new Feature { Name = name, Description = description?.Trim() };
        _db.Features.Add(f);
        await _db.SaveChangesAsync();

        if (selectedPlanIds is { Count: > 0 })
        {
            foreach (var pid in selectedPlanIds.Distinct())
                _db.PlanFeatures.Add(new PlanFeature { FeatureID = f.FeatureID, PlanID = pid });

            await _db.SaveChangesAsync();
        }

        return RedirectToAction("Index", "PADashboard", new { tab = "features" });
    }

    // Toggle the ShowOnHome flag from the table switch
    // POST /Features/ToggleHome
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleHome(int id, bool value)
    {
        var f = await _db.Features.FindAsync(id);
        if (f == null) return NotFound();

        f.ShowOnHome = value;
        await _db.SaveChangesAsync();

        return RedirectToAction("Index", "PADashboard", new { tab = "features" });
    }

    // POST /Features/Edit (from the View/Edit modal)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int featureId,
        string name,
        string? description,
        int? homeOrder,
        string? homeTitle,
        string? homeSummary,
        IFormFile? homeImage,                 // uploaded file
        string? existingHomeImagePath,        // keep old path if no upload
        List<int>? selectedPlanIds)
    {
        var feature = await _db.Features
            .Include(x => x.PlanFeatures)
            .FirstOrDefaultAsync(x => x.FeatureID == featureId);

        if (feature == null) return NotFound();

        feature.Name         = name?.Trim();
        feature.Description  = description?.Trim();
        feature.HomeOrder    = homeOrder;
        feature.HomeTitle    = homeTitle?.Trim();
        feature.HomeSummary  = homeSummary?.Trim();

        // Save uploaded image (if any) to /wwwroot/images/features/
        if (homeImage is { Length: > 0 })
        {
            var uploadsRoot = Path.Combine(_env.WebRootPath, "images", "features");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(homeImage.FileName)}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using (var fs = System.IO.File.Create(filePath))
                await homeImage.CopyToAsync(fs);

            // store as web path
            feature.HomeImagePath = $"/images/features/{fileName}";
        }
        else
        {
            // keep what UI sent as existing (or what’s already on the entity)
            if (!string.IsNullOrWhiteSpace(existingHomeImagePath))
                feature.HomeImagePath = existingHomeImagePath;
        }

        // Reconcile many-to-many PlanFeatures
        var newIds   = (selectedPlanIds ?? new List<int>()).Distinct().ToHashSet();
        var toRemove = feature.PlanFeatures.Where(pf => !newIds.Contains(pf.PlanID)).ToList();
        _db.PlanFeatures.RemoveRange(toRemove);

        var existing = feature.PlanFeatures.Select(pf => pf.PlanID).ToHashSet();
        foreach (var pid in newIds.Except(existing))
            _db.PlanFeatures.Add(new PlanFeature { FeatureID = feature.FeatureID, PlanID = pid });

        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "PADashboard", new { tab = "features" });
    }

    // POST /Features/Delete
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var feature = await _db.Features.FindAsync(id);
        if (feature == null) return NotFound();

        _db.Features.Remove(feature);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "PADashboard", new { tab = "features" });
    }
}
