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
    string? existingHomeImagePath,        // sent from hidden field to keep old path
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

    // 1. Store the old path BEFORE updating the feature object
    var oldHomeImagePath = feature.HomeImagePath;

    // 2. Save uploaded image (if any)
    if (homeImage is { Length: > 0 })
    {
        var uploadsRoot = Path.Combine(_env.WebRootPath, "images", "features");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(homeImage.FileName)}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        // Save the new file
        await using (var fs = System.IO.File.Create(filePath))
            await homeImage.CopyToAsync(fs);

        // Update the feature with the new path
        feature.HomeImagePath = $"/images/features/{fileName}";
        
        // 3. DELETE THE OLD FILE (if it exists and is not the new file)
        if (!string.IsNullOrWhiteSpace(oldHomeImagePath) && oldHomeImagePath != feature.HomeImagePath)
        {
            var oldFilePath = Path.Combine(_env.WebRootPath, oldHomeImagePath.TrimStart('/'));
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
        }
    }
    // 4. Handle case: No new file uploaded. 
    // This is correct as it keeps the existing DB value or accepts the hidden field value.
    else 
    {
        // If the existingHomeImagePath is blank/null, we assume the user intends to clear the image.
        // If the path is sent, keep it.
        feature.HomeImagePath = existingHomeImagePath; 
    }

    // Reconcile many-to-many PlanFeatures (existing logic remains)
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
