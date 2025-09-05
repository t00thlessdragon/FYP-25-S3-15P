using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using FYP_25_S3_15P.Models;
using FYP_25_S3_15P.Data;          // <-- DbContext namespace
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;


namespace FYP_25_S3_15P.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SmartDbContext _db;

    public HomeController(ILogger<HomeController> logger, SmartDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    // GET: /
    public IActionResult Index()
    {
        var plans = _db.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(p => p.Price)
            .ToList();
        return View(plans);   // pass IEnumerable<SubscriptionPlan> to the view
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