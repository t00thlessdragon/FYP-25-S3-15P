// Controllers/StudentController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using FYP_25_S3_15P.Data;          // SmartDbContext
using FYP_25_S3_15P.ViewModels;    // VMs

namespace FYP_25_S3_15P.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly SmartDbContext _db;
        public StudentController(SmartDbContext db) => _db = db;

        // GET /Student/Dashboard?tab=projects&q=...
        [HttpGet]
        public async Task<IActionResult> Dashboard(string? q)
        {
            // Pull published projects (optionally only those still open for preferences)
            var baseQuery = _db.Projects
                   .Where(p => p.PrefCloseAt == null || p.PrefCloseAt > DateTime.UtcNow)
                   .Select(p => new
                   {
                       p.ProjectId,
                       p.Title,
                       p.Description
                   });


            if (!string.IsNullOrWhiteSpace(q))
            {
                baseQuery = baseQuery.Where(p =>
                    p.ProjectId.Contains(q) ||
                    p.Title.Contains(q) ||
                    (p.Description ?? "").Contains(q));
            }

            var rows = await baseQuery
                .OrderBy(p => p.ProjectId)
                .ToListAsync();

            // Fetch tags (module names) for each project (simple second query; can be optimized)
            var projectIds = rows.Select(r => r.ProjectId).ToList();
            var tagsLookup = await _db.ProjectModules
                .Where(pm => projectIds.Contains(pm.ProjectId))
                .Join(_db.Modules, pm => pm.ModuleCode, m => m.ModuleCode,
                      (pm, m) => new { pm.ProjectId, m.ModuleName })
                .GroupBy(x => x.ProjectId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.ModuleName).ToList());

            var vm = new StudentDashboardVm
            {
                StudentName = User.Identity?.Name ?? "Jessy",
                StatusText  = "Allocation Pending",
                Search      = q,
                Projects    = rows.Select(r => new ProjectRowVm
                {
                    Id          = r.ProjectId,               // string
                    ProjectCode = r.ProjectId,
                    Title       = r.Title,
                    Description = r.Description ?? "",
                    Tags        = tagsLookup.TryGetValue(r.ProjectId, out var t) ? t : new List<string>(),
                    InPrefs     = false  // wire up when you implement preferences table
                }).ToList(),
                // Optional: build a reminder off the latest close date
                ReminderText = BuildReminderText(
    await _db.Projects.MaxAsync(p => (DateTime?)p.PrefCloseAt))

            };

            return View(vm); // Views/Student/Dashboard.cshtml
        }

        // GET /Student/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            var vm = new StudentProfileVm
            {
                Name  = User.Identity?.Name ?? "Jessy",
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "student@demo.com",
                Role  = "Student"
            };

            ViewBag.StudentName  = vm.Name;
            ViewBag.StudentEmail = vm.Email;
            ViewBag.StudentRole  = vm.Role;
            return View(vm);
        }

        // POST /Student/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(StudentProfileVm vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StudentName  = vm.Name;
                ViewBag.StudentEmail = vm.Email;
                ViewBag.StudentRole  = vm.Role;
                return View(vm);
            }

            TempData["Msg"] = "Profile saved.";
            return RedirectToAction(nameof(Profile));
        }

        // GET /Student/Projects  (Allocated Project page placeholder)
        [HttpGet]
        public IActionResult Projects()
        {
            var vm = new StudentProjectsVm();
            ViewBag.StudentName  = User.Identity?.Name ?? "Jessy";
            ViewBag.StudentEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "student@demo.com";
            ViewBag.StudentRole  = "Student";
            return View(vm); // Views/Student/Projects.cshtml
        }

        // GET /Student/Tasks (sample)
        [HttpGet]
        public IActionResult Tasks()
        {
            var vm = new StudentTasksVm
            {
                Tasks = new List<StudentTaskRowVm>
                {
                    new() { TaskId = "T01", Title = "Project requirement documentation", DueDate = new DateTime(2025, 8, 10), Status = "In Progress" },
                    new() { TaskId = "T02", Title = "System requirement specification",  DueDate = new DateTime(2025, 8, 20), Status = "In Progress" }
                }
            };

            ViewBag.StudentName  = User.Identity?.Name ?? "Jessy";
            ViewBag.StudentEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "student@demo.com";
            ViewBag.StudentRole  = "Student";
            return View(vm);
        }

        // POST from both modals (Dashboard + ProjectDetail)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToPreferences(string id, int? rank, string? from)
        {
            if (string.IsNullOrWhiteSpace(id) || rank is null || rank < 1)
            {
                TempData["Msg"] = "Please choose a rank.";
                return RedirectToAction(nameof(Dashboard), new { tab = "projects" });
            }

            // TODO: save student preference (studentId, projectId=id, rank.Value)
            TempData["Msg"] = $"Saved preference (rank {rank}) for {id}.";
            return RedirectToAction(nameof(Dashboard), new { tab = "prefs" });
        }

        // GET /Student/ViewDetails/{id}
        [HttpGet]
        public async Task<IActionResult> ViewDetails(string id, string? from)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var now = DateTime.UtcNow;
var p = await _db.Projects
    .Where(x => x.ProjectId == id &&
                (x.PrefCloseAt == null || x.PrefCloseAt > now))
    .Select(x => new
    {
        x.ProjectId,
        x.Title,
        x.Description
    })
    .FirstOrDefaultAsync();


            if (p == null) return NotFound();

            var tags = await _db.ProjectModules
                .Where(pm => pm.ProjectId == p.ProjectId)
                .Join(_db.Modules, pm => pm.ModuleCode, m => m.ModuleCode,
                      (pm, m) => m.ModuleName)
                .ToListAsync();

            var vm = new StudentProjectDetailsVm
            {
                Id              = p.ProjectId,   // string
                ProjectCode     = p.ProjectId,
                Title           = p.Title,
                DescriptionLead = p.Description ?? "",
                Functionalities = new List<string>(),
                DescriptionTail = "",
                Tags            = tags
            };

            return View("ProjectDetail", vm); // Views/Student/ProjectDetail.cshtml
        }

        [HttpGet]
        public IActionResult Meetings()
        {
            var vm = new StudentMeetingsVm
            {
                Meetings = new List<MeetingRowVm>
                {
                    new() { MeetingId = "M01", Title = "Supervisor Consultation", Date = new DateTime(2025, 7, 8), TimeText = "2:00PM", Organizer = "Mr. Prem" }
                }
            };

            ViewBag.StudentName  = User.Identity?.Name ?? "Jessy";
            ViewBag.StudentEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "student@demo.com";
            ViewBag.StudentRole  = "Student";
            return View(vm);
        }

        private static string BuildReminderText(DateTime? closesAtUtc)
        {
            if (closesAtUtc is null) return "Preferences are currently open.";
            var local = closesAtUtc.Value.ToLocalTime();
            return $"Preferences close on {local:dd MMM yyyy, HH:mm}.";
        }
    }
}
