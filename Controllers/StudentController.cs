// Controllers/StudentController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FYP_25_S3_15P.ViewModels;

namespace FYP_25_S3_15P.Controllers
{
    // Conventional routing → /Student/{Action}
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        // GET /Student/Dashboard
        [HttpGet]
        public IActionResult Dashboard(string? q)
        {
            var data = new List<ProjectRowVm>
            {
                new()
                {
                    Id = 1,
                    ProjectCode = "CSIT-25-S3-08",
                    Title = "A Smart Project Allocation System",
                    Description = "Multi-constraint optimisation for fair allocation.",
                    Tags = new() { "Allocation", "System" }
                },
                new()
                {
                    Id = 2,
                    ProjectCode = "CSIT-25-S3-09",
                    Title = "Identifying cryptographic functions",
                    Description = "Survey and identify crypto libs used in systems.",
                    Tags = new() { "Blockchain", "Security" }
                }
            }.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                data = data.Where(p =>
                    p.ProjectCode.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    p.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            var vm = new StudentDashboardVm
            {
                StudentName = User.Identity?.Name ?? "Jessy",
                StatusText  = "Allocation Pending",
                Search      = q,
                Projects    = data.ToList()
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

            return View(vm); // Views/Student/Profile.cshtml
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

            // TODO: save changes later
            TempData["Msg"] = "Profile saved.";
            return RedirectToAction(nameof(Profile));
        }

        // GET /Student/Projects
        [HttpGet]
        public IActionResult Projects()
        {
            var vm = new StudentProjectsVm(); // empty → placeholder message in view

            ViewBag.StudentName  = User.Identity?.Name ?? "Jessy";
            ViewBag.StudentEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "student@demo.com";
            ViewBag.StudentRole  = "Student";

            return View(vm); // Views/Student/Projects.cshtml
        }

        // GET /Student/Tasks (sample rows)
        [HttpGet]
        public IActionResult Tasks()
        {
            var vm = new StudentTasksVm
            {
                Tasks = new List<StudentTaskRowVm>
                {
                    new()
                    {
                        TaskId  = "T01",
                        Title   = "Project requirement documentation",
                        DueDate = new DateTime(2025, 8, 10),
                        Status  = "In Progress"
                    },
                    new()
                    {
                        TaskId  = "T02",
                        Title   = "System requirement specification",
                        DueDate = new DateTime(2025, 8, 20),
                        Status  = "In Progress"
                    }
                }
            };

            ViewBag.StudentName  = User.Identity?.Name ?? "Jessy";
            ViewBag.StudentEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "student@demo.com";
            ViewBag.StudentRole  = "Student";

            return View(vm); // Views/Student/Tasks.cshtml
        }

        // POST /Student/AddToPreferences
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToPreferences(int id)
        {
            TempData["Msg"] = $"Project {id} added.";
            return RedirectToAction(nameof(Dashboard));
        }

        // GET /Student/ViewDetails/5
        // GET /Student/ViewDetails/5?from=projects
public IActionResult ViewDetails(int id, string? from)
{
    // Hardcoded sample content – swap for DB later
    var vm = new StudentProjectDetailsVm
    {
        Id = id,
        ProjectCode = "CSIT-25-S3-08",
        Title = "A Smart Project Allocation System",
        DescriptionLead =
            "SIM, a long standing partner of UOW, has over 3,000 students pursuing " +
            "UOW’s Bachelor of Computer Science, Bachelor of IT, and Bachelor of " +
            "Business Information Systems. They need a system to assist managing CSIT321 projects.",
        Functionalities = new List<string>
        {
            "Project listing with constraint specification.",
            "Project viewing for students/supervisors based on fit.",
            "Project bidding to express preferences.",
            "Automatic project allocation optimising multiple objectives.",
            "Project reporting with historical analytics."
        },
        DescriptionTail =
            "The system aims to solve a multiconstraint optimisation problem and streamline allocation.",
        Tags = new() { "Allocation", "System" }
    };

    ViewBag.From = from; // not necessary, we read from querystring; kept for clarity
    return View("ProjectDetails", vm);
}


        // GET /Student/Meetings (sample row)
   
[HttpGet]
public IActionResult Meetings()
{
    var vm = new StudentMeetingsVm
    {
        Meetings = new List<MeetingRowVm>
        {
            new()
            {
                MeetingId = "M01",
                Title     = "Supervisor Consultation",
                Date      = new DateTime(2025, 7, 8),
                TimeText  = "2:00PM",         // <-- use TimeText (match the model)
                Organizer = "Mr. Prem"
            }
        }
    };

    ViewBag.StudentName  = User.Identity?.Name ?? "Jessy";
    ViewBag.StudentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "student@demo.com";
    ViewBag.StudentRole  = "Student";

    return View(vm); // Views/Student/Meetings.cshtml
}


    }
}
