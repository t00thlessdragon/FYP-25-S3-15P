using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FYP_25_S3_15P.Controllers
{
    public class ApplicationFormController : Controller
    {
        private readonly SmartDbContext _db;
        public ApplicationFormController(SmartDbContext db) => _db = db;

        // ----- ViewModel used only for the form -----
        public class ApplicationFormVm
        {
            [Required, StringLength(120)]
            public string Name { get; set; } = string.Empty;

            [Required, EmailAddress, StringLength(254)]
            public string Email { get; set; } = string.Empty;

            [Required, StringLength(200)]
            public string UniversityName { get; set; } = string.Empty;

            [Required, StringLength(50)]
            public string UEN { get; set; } = string.Empty;

            [Required, StringLength(50)]
            public string Role { get; set; } = string.Empty;

            [Required]
            public int PlanID { get; set; }

            public IEnumerable<SelectListItem> Plans { get; set; } = Enumerable.Empty<SelectListItem>();
            public IEnumerable<SelectListItem> Roles { get; set; } = new[]
            {
                new SelectListItem("PlatformAdmin", "PlatformAdmin"),
                new SelectListItem("UniversityAdmin", "UniversityAdmin"),
                new SelectListItem("Coordinator", "Coordinator")
            };
        }

        // GET: /ApplicationForm
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var plans = await _db.SubscriptionPlans
                                 .AsNoTracking()
                                 .OrderBy(p => p.Price)
                                 .Select(p => new SelectListItem(p.Name, p.PlanID.ToString()))
                                 .ToListAsync();

            var vm = new ApplicationFormVm { Plans = plans };
            return View("~/Views/Forms/ApplicationForm.cshtml", vm);
        }

        // POST: /ApplicationForm
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ApplicationFormVm input)
        {
            if (!ModelState.IsValid)
            {
                input.Plans = await _db.SubscriptionPlans.AsNoTracking()
                    .OrderBy(p => p.Price)
                    .Select(p => new SelectListItem(p.Name, p.PlanID.ToString()))
                    .ToListAsync();
                return View("~/Views/Forms/ApplicationForm.cshtml", input);
            }

            var uniName = input.UniversityName.Trim();
            var uen     = input.UEN.Trim().ToUpperInvariant();

            await using var tx = await _db.Database.BeginTransactionAsync();

            // Upsert by UEN (unique)
            var uni = await _db.Universities.FirstOrDefaultAsync(u => u.UEN == uen);
            if (uni is null)
            {
                uni = new University
                {
                    UniName   = uniName,
                    UEN       = uen,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Universities.Add(uni);
                await _db.SaveChangesAsync(); // get UniID
            }
            else if (!string.Equals(uni.UniName, uniName, StringComparison.Ordinal))
            {
                // keep UEN as source of truth; refresh name if changed
                uni.UniName = uniName;
                await _db.SaveChangesAsync();
            }

            var app = new ApplicationForm
            {
                ApplicantName = input.Name.Trim(),
                Email         = input.Email.Trim(),
                Role          = input.Role,
                PlanID        = input.PlanID,
                UniID         = uni.UniID,
                Status        = "Pending"
            };

            _db.ApplicationForms.Add(app);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            TempData["AppSuccess"] = true;
            return RedirectToAction(nameof(Index));
        }
    }
}
