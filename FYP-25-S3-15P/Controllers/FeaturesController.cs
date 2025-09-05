using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Controllers
{
    public class FeaturesController : Controller
    {
        private readonly SmartDbContext _db;
        public FeaturesController(SmartDbContext db) => _db = db;

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Feature f, int[] selectedPlanIds)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "PADashboard", new { tab = "features" });

            _db.Features.Add(f);
            await _db.SaveChangesAsync(); // get FeatureID

            foreach (var pid in selectedPlanIds.Distinct())
                _db.PlanFeatures.Add(new PlanFeature { PlanID = pid, FeatureID = f.FeatureID });

            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "PADashboard", new { tab = "features" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("FeatureID,Name,Description")] Feature f, int[] selectedPlanIds)
        {
            var existing = await _db.Features
                .Include(x => x.PlanFeatures)
                .FirstOrDefaultAsync(x => x.FeatureID == f.FeatureID);

            if (existing == null) return NotFound();

            existing.Name = f.Name;
            existing.Description = f.Description;

            var newSet       = selectedPlanIds?.ToHashSet() ?? new HashSet<int>();
            var currentSet   = existing.PlanFeatures.Select(pf => pf.PlanID).ToHashSet();

            // remove unselected
            _db.PlanFeatures.RemoveRange(existing.PlanFeatures.Where(pf => !newSet.Contains(pf.PlanID)).ToList());

            // add newly selected
            foreach (var pid in newSet.Except(currentSet))
                existing.PlanFeatures.Add(new PlanFeature { PlanID = pid, FeatureID = existing.FeatureID });

            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "PADashboard", new { tab = "features" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var f = await _db.Features.FindAsync(id);
            if (f != null)
            {
                _db.Features.Remove(f);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index", "PADashboard", new { tab = "features" });
        }
    }
}