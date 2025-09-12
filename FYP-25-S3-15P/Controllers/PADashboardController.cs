using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;      // ContentMaster, Feature, SubscriptionPlan
using FYP_25_S3_15P.ViewModels;  // ApplicationMasterVm

namespace FYP_25_S3_15P.Controllers
{
    public class PADashboardController : Controller
    {
        private readonly SmartDbContext _db;
        public PADashboardController(SmartDbContext db) => _db = db;

        // Default – content dashboard
        [HttpGet("/PADashboard")]
        public async Task<IActionResult> ContentMaster()
        {
            var features = await _db.Features
                .Include(f => f.PlanFeatures)
                    .ThenInclude(pf => pf.Plan)
                .OrderBy(f => f.Name)
                .ToListAsync();

            var plans = await _db.SubscriptionPlans
                .OrderBy(p => p.Price)
                .ToListAsync();

            var model = new ContentMaster
            {
                Features = features,
                Plans = plans
            };

            return View("~/Views/Dashboards/PA/ContentMaster.cshtml", model);
        }

        // Explicit alias
        [HttpGet("/PADashboard/ContentMaster")]
        public Task<IActionResult> ContentMasterAlias() => ContentMaster();

        // Applications dashboard
        [HttpGet("/PADashboard/ApplicationMaster")]
        public async Task<IActionResult> ApplicationMaster()
        {
            var rows = await (
                from a in _db.ApplicationForms
                join u in _db.Universities       on a.UniID  equals u.UniID
                join p in _db.SubscriptionPlans  on a.PlanID equals p.PlanID
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
