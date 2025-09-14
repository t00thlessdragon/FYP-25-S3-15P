using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;                 // 👈 add this

using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;      // ContentMaster, Feature, SubscriptionPlan
using FYP_25_S3_15P.ViewModels;  // ApplicationMasterVm

namespace FYP_25_S3_15P.Controllers
{
    // ✅ Only Platform Admins can access anything in this controller
    [Authorize(Roles = "Platform Admin")]
    public class PADashboardController : Controller
    {
        private readonly SmartDbContext _db;
        public PADashboardController(SmartDbContext db) => _db = db;

        // Conventional route for links/redirects
        [HttpGet]
        public Task<IActionResult> Index([FromQuery] string? tab) => ContentMaster(tab);

        // Content dashboard (reads ?tab=)
        [HttpGet("/PADashboard")]
        public async Task<IActionResult> ContentMaster([FromQuery] string? tab)
        {
            var features = await _db.Features
                .Include(f => f.PlanFeatures).ThenInclude(pf => pf.Plan)
                .OrderBy(f => f.Name)
                .ToListAsync();

            var plans = await _db.SubscriptionPlans
                .OrderBy(p => p.Price)
                .ToListAsync();

            var model = new ContentMaster
            {
                Features  = features,
                Plans     = plans,
                ActiveTab = string.IsNullOrWhiteSpace(tab) ? "features" : tab.ToLowerInvariant()
            };

            return View("~/Views/Dashboards/PA/ContentMaster.cshtml", model);
        }
        
        // Applications dashboard
        [HttpGet("/PADashboard/ApplicationMaster")]
        public async Task<IActionResult> ApplicationMaster()
        {
            var rows = await (
                from a in _db.ApplicationForms
                join u in _db.Universities      on a.UniID  equals u.UniID
                join p in _db.SubscriptionPlans on a.PlanID equals p.PlanID
                orderby a.CreatedAt descending
                select new ApplicationMasterVm.Row
                {
                    AppId         = a.AppId,
                    ApplicantName = a.ApplicantName,
                    Email         = a.Email,
                    Role          = a.Role,
                    UniName       = u.UniName,
                    UEN           = u.UEN,
                    PlanId        = p.PlanID,
                    PlanName      = p.Name,
                    Status        = a.Status,
                    CreatedAt     = a.CreatedAt
                }
            ).ToListAsync();

            var vm = new ApplicationMasterVm { Applications = rows };
            return View("~/Views/Dashboards/PA/ApplicationMaster.cshtml", vm);
        }
    }
}
