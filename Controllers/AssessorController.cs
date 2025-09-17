// Controllers/AssessorController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FYP_25_S3_15P.ViewModels;

namespace FYP_25_S3_15P.Controllers
{
    [Authorize(Roles = "Assessor")]
    public class AssessorController : Controller
    {
        // GET /Assessor/Dashboard
        [HttpGet]
        public IActionResult Dashboard(string? q, string? tab)
        {
            // Use List<T> (not IQueryable) to avoid expression-tree rules
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
            };

            if (!string.IsNullOrWhiteSpace(q))
            {
                var cmp = StringComparison.OrdinalIgnoreCase;
                data = data.Where(p =>
                           (!string.IsNullOrEmpty(p.ProjectCode)  && p.ProjectCode.Contains(q, cmp)) ||
                           (!string.IsNullOrEmpty(p.Title)        && p.Title.Contains(q, cmp)) ||
                           (!string.IsNullOrEmpty(p.Description)  && p.Description.Contains(q, cmp)))
                           .ToList();
            }

            var vm = new AssessorDashboardVm
            {
                AssessorName = User.Identity?.Name ?? "Demo Assessor",
                Search       = q,
                Projects     = data
            };

            return View(vm); // Views/Assessor/Dashboard.cshtml
        }

        // GET /Assessor/Projects?q=...
        [HttpGet]
        public IActionResult Projects(string? q)
        {
            var list = new List<AssessorProjectsVM>
            {
                new()
                {
                    No = 1,
                    ProjectId    = "CSIT-25-S3-08",
                    ProjectTitle = "A Smart Project Allocation System",
                    GroupId      = "FYP-25-S3-15P",
                    Supervisor   = "Mr Prem",
                    Status       = "Active",
                    ProjectKey   = 8
                },
                new()
                {
                    No = 2,
                    ProjectId    = "CSIT-25-S3-09",
                    ProjectTitle = "Identifying cryptographic functions",
                    GroupId      = "FYP-25-S3-20P",
                    Supervisor   = "Mr Low",
                    Status       = "Completed",
                    ProjectKey   = 9
                }
            };

            if (!string.IsNullOrWhiteSpace(q))
            {
                var cmp = StringComparison.OrdinalIgnoreCase;
                list = list.Where(p =>
                           (!string.IsNullOrEmpty(p.ProjectId)    && p.ProjectId.Contains(q, cmp)) ||
                           (!string.IsNullOrEmpty(p.ProjectTitle) && p.ProjectTitle.Contains(q, cmp)) ||
                           (!string.IsNullOrEmpty(p.GroupId)      && p.GroupId.Contains(q, cmp)) ||
                           (!string.IsNullOrEmpty(p.Supervisor)   && p.Supervisor.Contains(q, cmp)) ||
                           (!string.IsNullOrEmpty(p.Status)       && p.Status.Contains(q, cmp)))
                           .ToList();
            }

            ViewData["Search"] = q; // optional
            return View(list); // Views/Assessor/Projects.cshtml
        }

        [HttpGet]
        public IActionResult ViewProject(int id) =>
            Content($"Assessor view of project {id} (placeholder)");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkForEvaluation(int id)
        {
            TempData["Msg"] = $"Project {id} marked for evaluation.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
public IActionResult ProjectDetails(int id)
{
    // Mock data — swap to DB later
    AssessorProjectDetailsVm? vm = id switch
    {
        8 => new AssessorProjectDetailsVm
        {
            ProjectId   = "CSIT-25-S3-08",
            Title       = "A Smart Project Allocation System",
            GroupId     = "FYP-25-S3-15P",
            Supervisor  = "Mr. Prem",
            Assessor    = "Mr. Tan",
            Members     = new() { "Jessy", "Doraine", "Alan" },
            Description = "SIM… managing CSIT321 projects. The system aims to solve a multiconstraint optimisation problem."
        },
        9 => new AssessorProjectDetailsVm
        {
            ProjectId   = "CSIT-25-S3-09",
            Title       = "Identifying cryptographic functions",
            GroupId     = "FYP-25-S3-20P",
            Supervisor  = "Mr. Low",
            Assessor    = "Mr. Tan",
            Members     = new() { "Ming", "Syafiq", "Hanna" },
            Description = "Survey and identify cryptographic libraries used in systems."
        },
        _ => null
    };

    if (vm == null) return RedirectToAction(nameof(Projects));
    return View("~/Views/Assessor/ProjectDetails.cshtml", vm);
}


// GET /Assessor/Tasks?projectId=CSIT-25-S3-08
[HttpGet]
[ActionName("TasksByProject")]
public IActionResult Tasks(string projectId)
{
    if (string.IsNullOrWhiteSpace(projectId))
        return RedirectToAction(nameof(Projects));

    // --- sample “project header” data, keyed by ProjectId ---
    var headers = new Dictionary<string, AssessorProjectDetailsVm>(StringComparer.OrdinalIgnoreCase)
    {
        ["CSIT-25-S3-08"] = new AssessorProjectDetailsVm
        {
            ProjectId = "CSIT-25-S3-08",
            Title     = "A Smart Project Allocation System",
            GroupId   = "FYP-25-S3-15P",
            Supervisor= "Mr. Prem",
            Assessor  = "Mr. Tan",
            Members   = new() { "Jessy", "Doraine", "Alan" },
            Description = "SIM… managing CSIT321 projects. The system aims to solve a multiconstraint optimisation problem."
        },
        ["CSIT-25-S3-09"] = new AssessorProjectDetailsVm
        {
            ProjectId = "CSIT-25-S3-09",
            Title     = "Identifying cryptographic functions",
            GroupId   = "FYP-25-S3-20P",
            Supervisor= "Mr. Low",
            Assessor  = "Ms. Lee",
            Members   = new() { "Ivy", "Ben" },
            Description = "Survey and identify crypto libraries used in systems."
        }
    };

    if (!headers.TryGetValue(projectId, out var hdr))
        return RedirectToAction(nameof(Projects));

    // --- sample tasks per project ---
    List<AssessorTaskRowVm> tasks = projectId.Equals("CSIT-25-S3-08", StringComparison.OrdinalIgnoreCase)
        ? new List<AssessorTaskRowVm>
        {
            new() { TaskId = "T01", Title = "Project requirement documentation",
                    SubmittedOn = new DateTime(2025, 8, 15), DueDate = new DateTime(2025, 8, 16), Status = "Submitted" },
            new() { TaskId = "T02", Title = "System requirement specification",
                    SubmittedOn = new DateTime(2025, 8, 17), DueDate = new DateTime(2025, 8, 18), Status = "Submitted" },
        }
        : new List<AssessorTaskRowVm>
        {
            new() { TaskId = "T01", Title = "Dataset curation report",
                    SubmittedOn = new DateTime(2025, 8, 12), DueDate = new DateTime(2025, 8, 13), Status = "Submitted" },
            new() { TaskId = "T02", Title = "Crypto functions survey",
                    SubmittedOn = new DateTime(2025, 8, 20), DueDate = new DateTime(2025, 8, 22), Status = "Pending" },
        };

    var vm = new AssessorTaskDeliverablesVm
    {
        ProjectId = hdr.ProjectId,
        Title     = hdr.Title,
        GroupId   = hdr.GroupId,
        Supervisor= hdr.Supervisor,
        Assessor  = hdr.Assessor,
        Members   = hdr.Members,
        Tasks     = tasks
    };

    return View("~/Views/Assessor/Tasks.cshtml", vm);
}


// GET /Assessor/TaskInfo?projectId=CSIT-25-S3-08&taskId=T01
[HttpGet]
public IActionResult TaskInfo(string projectId, string taskId)
{
    // sample data to mirror your Deliverables rows
    var all = new List<AssessorTaskInfoVm>
    {
        new AssessorTaskInfoVm
        {
            ProjectId = "CSIT-25-S3-08",
            GroupId   = "FYP-25-S3-15P",
            TaskId    = "T01",
            Title     = "Project requirement documentation",
            SubmissionDate = new DateTime(2025, 7, 9),
            DueDate        = new DateTime(2025, 8, 16),
            Status   = "Submitted",
            FileName = "Project_Requirement_Doc.pdf",
            FileUrl  = "/files/Project_Requirement_Doc.pdf" // placeholder
        },
        new AssessorTaskInfoVm
        {
            ProjectId = "CSIT-25-S3-08",
            GroupId   = "FYP-25-S3-15P",
            TaskId    = "T02",
            Title     = "System requirement specification",
            SubmissionDate = new DateTime(2025, 7, 17),
            DueDate        = new DateTime(2025, 8, 18),
            Status   = "Submitted",
            FileName = "System_Requirement_Spec.pdf",
            FileUrl  = "/files/System_Requirement_Spec.pdf"
        }
    };

    var vm = all.FirstOrDefault(t =>
                string.Equals(t.ProjectId, projectId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.TaskId, taskId, StringComparison.OrdinalIgnoreCase));

    if (vm == null)
        return NotFound("Task not found for this project.");

    return View(vm); // Views/Assessor/TaskInfo.cshtml
}

// GET /Assessor/Evaluate?projectId=...&taskId=...&from=project|list
[HttpGet]
public IActionResult Evaluate(string projectId, string taskId, string? from)
{
    // existing sample data...
    var vm = new FYP_25_S3_15P.ViewModels.AssessorEvaluateVm
    {
        ProjectId = projectId,
        TaskId    = taskId,
        Title     = "Project requirement documentation",
        GroupId   = "FYP-25-S3-15P",
        SubmittedOn = new DateTime(2025, 7, 9),
        DeliverableWeightPercent = 10,
        Items = new List<FYP_25_S3_15P.ViewModels.RubricItemVm>
        {
            new() { Criterion = "Quality",        Description = "Code/content quality, clarity", Weight = 50, Score = 8 },
            new() { Criterion = "Completeness",   Description = "Meets all requirements",       Weight = 25, Score = 7 },
            new() { Criterion = "Documentation",  Description = "Readme, comments, references", Weight = 25, Score = 9 },
        }
    };

    ViewBag.From = from;   // remember source (project | list)
    return View("~/Views/Assessor/Evaluate.cshtml", vm);
}

// POST /Assessor/Evaluate
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Evaluate(FYP_25_S3_15P.ViewModels.AssessorEvaluateVm vm, string? from)
{
    // TODO: save rubric results
    TempData["Msg"] = $"Scores saved for {vm.ProjectId} - {vm.TaskId}. " +
                      $"Deliverable score: {vm.TotalScore}/100 (→ {vm.ContributionPercent}% of project).";

    // Go back where the user came from
    if (string.Equals(from, "list", StringComparison.OrdinalIgnoreCase))
        return RedirectToAction(nameof(TaskDeliverables));          // Tasks Deliverables Listing

    // default: came from the project’s task page
    return RedirectToAction(nameof(Tasks), new { projectId = vm.ProjectId });
}


[HttpGet]
public IActionResult TaskDeliverables(string? q, string? status)
{
    // sample rows (wire to DB later)
    var rows = new List<AssessorTaskDeliverableRowVm>
    {
        new()
        {
            No = 1,
            GroupId = "FYP-25-S3-15P",
            ProjectId = "CSIT-25-S3-08",
            ProjectTitle = "A Smart Project Allocation System",
            TaskId = "T01",
            TaskTitle = "Project Requirement Documentation",
            SubmittedOn = new DateTime(2025, 8, 15),
            DueDate = new DateTime(2025, 8, 16),
            Status = "Evaluation Saved",
            FileUrl = "/files/Project_Requirement_Doc.pdf"
        },
        new()
        {
            No = 2,
            GroupId = "FYP-25-S3-20P",
            ProjectId = "CSIT-25-S3-09",
            ProjectTitle = "Identifying cryptographic functions",
            TaskId = "T01",
            TaskTitle = "Project Requirement Documentation",
            SubmittedOn = new DateTime(2025, 8, 17),
            DueDate = new DateTime(2025, 8, 18),
            Status = "Pending Evaluation",
            FileUrl = "/files/Project_Requirement_Doc.pdf"
        }
    };

    // simple search/filter (title, group, project, status)
    if (!string.IsNullOrWhiteSpace(q))
    {
        var cmp = StringComparison.OrdinalIgnoreCase;
        rows = rows.Where(r =>
                 r.GroupId.Contains(q, cmp) ||
                 r.ProjectId.Contains(q, cmp) ||
                 r.ProjectTitle.Contains(q, cmp) ||
                 r.TaskTitle.Contains(q, cmp) ||
                 r.Status.Contains(q, cmp))
               .ToList();
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
        rows = rows.Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    ViewBag.Search = q;
    ViewBag.Filter = status;
    return View(rows); // -> Views/Assessor/TaskDeliverables.cshtml
}


[HttpGet]
public IActionResult Tasks()
{
    return RedirectToAction(nameof(TaskDeliverables));
}


// GET /Assessor/Assessment?q=...
[HttpGet]
public IActionResult Assessment(string? q)
{
    var rows = new List<AssessorAssessmentRowVm>
    {
        new() {
            No=1, GroupId="FYP-25-S3-15P", ProjectId="CSIT-25-S3-08",
            ProjectTitle="A Smart Project Allocation System",
            Status="Pending", SubmissionDate=new DateTime(2025,8,15), MarksAwarded="0%"
        },
        new() {
            No=2, GroupId="FYP-25-S3-20P", ProjectId="CSIT-25-S3-09",
            ProjectTitle="Identifying cryptographic functions",
            Status="Submitted", SubmissionDate=new DateTime(2025,8,17), MarksAwarded="80%"
        }
    };

    if (!string.IsNullOrWhiteSpace(q))
    {
        var cmp = StringComparison.OrdinalIgnoreCase;
        rows = rows.Where(r =>
               r.GroupId.Contains(q, cmp) ||
               r.ProjectId.Contains(q, cmp) ||
               r.ProjectTitle.Contains(q, cmp) ||
               r.Status.Contains(q, cmp))
            .ToList();
    }

    ViewBag.Search = q;
    return View("~/Views/Assessor/Assessment.cshtml", rows);
}

// GET /Assessor/SubmitAssessment?projectId=CSIT-25-S3-08
[HttpGet]
public IActionResult SubmitAssessment(string projectId, string? from = "assessment")
{
    // Mock data
    var vm = new SubmitAssessmentVm
    {
        ProjectId = projectId,
        ProjectTitle = "A Smart Project Allocation System",
        GroupId = "FYP-25-S3-15P",
        Supervisor = "Mr. Prem",
        Assessor = "Mr. Tan",
        Members = new() { "Jessy", "Doraine", "Alan" },
        Tasks = new()
        {
            new() { TaskId="T01", TaskTitle="Project requirement documentation", ScoreOutOf100=80, ContributionPercent=10 },
            new() { TaskId="T02", TaskTitle="System requirement specification",   ScoreOutOf100=75, ContributionPercent=15 },
        }
    };
    vm.TotalContributionPercent = 8.0m + 11.25m; // as in your design

    ViewBag.From = from; // so we can return to the correct page
    return View("~/Views/Assessor/SubmitAssessment.cshtml", vm);
}

// POST /Assessor/SubmitAssessment
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult SubmitAssessment(SubmitAssessmentVm vm, string? from = "assessment")
{
    // TODO: persist assessment submission
    TempData["Msg"] = $"Assessment submitted for {vm.ProjectId}. Total contribution: {vm.TotalContributionPercent}%.";

    // Go back to the correct listing page
    if (string.Equals(from, "tasks", StringComparison.OrdinalIgnoreCase))
        return RedirectToAction(nameof(TaskDeliverables));
    return RedirectToAction(nameof(Assessment));
}


[HttpGet]
public IActionResult Profile()
{
    var vm = new FYP_25_S3_15P.ViewModels.AssessorProfileVm
    {
        Name = User.Identity?.Name ?? "Demo Assessor"
        // load the rest from DB if you have it
    };
    return View("~/Views/Assessor/Profile.cshtml", vm);
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Profile(FYP_25_S3_15P.ViewModels.AssessorProfileVm vm)
{
    if (!ModelState.IsValid)
    {
        return View("~/Views/Assessor/Profile.cshtml", vm);
    }

    // TODO: persist Email, Phone, and vm.Expertise (parsed from ExpertiseCsv)
    TempData["Msg"] = "Profile updated.";
    return RedirectToAction(nameof(Profile));
}

    }
}
