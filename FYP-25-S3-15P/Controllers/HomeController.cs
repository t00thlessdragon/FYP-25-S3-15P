using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using FYP_25_S3_15P.ViewModels;

namespace FYP_25_S3_15P.Controllers
{
    public class HomeController : Controller
    {
        private readonly SmartDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(SmartDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            var vm = new HomeLandingVm
            {
                Plans = await _db.SubscriptionPlans
                    .AsNoTracking()
                    .OrderBy(p => p.Price)
                    .ToListAsync(),

                // Features flagged to show on homepage, ordered by HomeOrder then Name
                HomeFeatures = await _db.Features
                    .AsNoTracking()
                    .Where(f => f.ShowOnHome)
                    .OrderBy(f => f.HomeOrder ?? int.MaxValue)
                    .ThenBy(f => f.Name)
                    .ToListAsync()
            };

            return View(vm);
        }

        // GET: /Home/About
        public IActionResult About()
        {
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}