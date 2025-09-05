using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Controllers
{
    // [Authorize(Roles="PlatformAdmin")]
    public class PADashboardController : Controller
    {
        private readonly SmartDbContext _db;
        public PADashboardController(SmartDbContext db) => _db = db;

        // GET /PADashboard?tab=features
        public async Task<IActionResult> Index(string tab = "features")
        {
            var plans = await _db.SubscriptionPlans
                .AsNoTracking()
                .OrderBy(p => p.Price)
                .ToListAsync();

            var features = await _db.Features
                .AsNoTracking()
                .Include(f => f.Plan)
                .OrderBy(f => f.Name)
                .ToListAsync();

            var model = new ContentMaster { ActiveTab = tab };

            if (tab == "features")
            {
                model.Plans = await _db.SubscriptionPlans
                    .AsNoTracking()
                    .OrderBy(p => p.Price)
                    .ToListAsync();

                model.Features = await _db.Features
                    .AsNoTracking()
                    .Include(f => f.PlanFeatures)
                    .ThenInclude(pf => pf.Plan)
                    .OrderBy(f => f.Name)
                    .ToListAsync();
            }
            else if (tab == "plans")
            {
                model.Plans = await _db.SubscriptionPlans
                    .AsNoTracking()
                    .OrderBy(p => p.Price)
                    .ToListAsync();
            }

            return View("~/Views/Dashboards/PA/ContentMaster.cshtml", model);

        }
    }
}