using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using FYP_25_S3_15P.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DbCourse  = FYP_25_S3_15P.Models.Course;
using DbModule  = FYP_25_S3_15P.Models.Module;
// Aliases for core DB models
using DbProgram = FYP_25_S3_15P.Models.Program;

namespace FYP_25_S3_15P.Controllers
{
    // Enforcing role-based access for the entire controller.
    // This is stricter than the [Authorize] on the original UADashboard.
    [Authorize(Roles = "University Admin")]
    public class UADashboardController : Controller
    {
        private readonly SmartDbContext _db;
        public UADashboardController(SmartDbContext db) => _db = db;

        // ====================================================================
        // USERS & DASHBOARD (from UADashboardController.cs)
        // ====================================================================

        public IActionResult Index()
        {
            return View("~/Views/Dashboards/UA/Dashboard.cshtml");
        }
    
        public IActionResult University()
        {
        return View("~/Views/Dashboards/UA/University.cshtml"); 
        }

        public async Task<IActionResult> UserMaster()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !int.TryParse(userId, out int adminId))
            {
                TempData["Error"] = "User identity not found or invalid.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var adminProfile = await _db.Users
                .Include(p => p.University)
                .FirstOrDefaultAsync(p => p.Id == adminId);

            // The class-level [Authorize(Roles = "UniversityAdmin")] should handle most of this,
            // but this internal check provides a secondary defense and fetches Uni details.
            const int UniversityAdminRoleID = 9;

            if (adminProfile?.RoleId != UniversityAdminRoleID)
            {
                TempData["Error"] = "Insufficient privileges for this dashboard.";
                return RedirectToAction("AccessDenied", "Account");
            }

            if (adminProfile?.University == null)
            {
                TempData["Error"] = "University profile information is missing.";
                return View("~/Views/Dashboards/UA/UserMaster.cshtml", new StaffAndStudentVm());
            }

            var uniAbbrv = adminProfile.University.UniAbbrv ?? string.Empty;
            var uniId = adminProfile.University.UniID;

            var viewModel = new StaffAndStudentVm
            {
                Staff = await GetStaffDataAsync(uniId, uniAbbrv),
                Students = await GetStudentDataAsync(uniId, uniAbbrv)
            };

            // Using the view path specified in the original UADashboardController
            return View("~/Views/Dashboards/UA/UserMaster.cshtml", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !int.TryParse(userId, out int adminId))
            {
                TempData["Error"] = "User identity not found or invalid.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var admin = await _db.Users
                .Include(u => u.University)
                .FirstOrDefaultAsync(u => u.Id == adminId);

            const int UniversityAdminRoleID = 9;
            if (admin?.RoleId != UniversityAdminRoleID)
            {
                TempData["Error"] = "Insufficient privileges for this dashboard.";
                return RedirectToAction("AccessDenied", "Account");
            }

            if (admin?.University == null)
            {
                TempData["Error"] = "University profile information is missing.";
                return View("~/Views/Dashboards/UA/Profile.cshtml", new UAProfileVm());
            }

            var uniId = admin!.University!.UniID;

            // Get StaffProfile for this admin
            var staffProfile = await _db.StaffProfiles
                .FirstOrDefaultAsync(sp => sp.UserID == adminId);

            // Count students
            var totalStudents = await _db.StudentProfiles
                .CountAsync(sp => sp.User != null && sp.User.UniID == uniId);

            // Role-based counts for staff
            var totalAssessors = await _db.StaffProfiles
                .CountAsync(sp => sp.User != null && sp.User.UniID == uniId && sp.User.RoleId == 8);

            var totalSupervisors = await _db.StaffProfiles
                .CountAsync(sp => sp.User != null && sp.User.UniID == uniId && sp.User.RoleId == 5);

            var totalSubjectCoordinators = await _db.StaffProfiles
                .CountAsync(sp => sp.User != null && sp.User.UniID == uniId && sp.User.RoleId == 3);

            // Catalog counts
            var (totalPrograms, totalCourses, totalModules) = await GetCatalogCountsAsync(uniId);

            var vm = new UAProfileVm
            {
                Name = admin.Name ?? "Unknown",
                Email = admin.Email ?? "",
                StaffID = staffProfile?.StaffID ?? "N/A",
                AvatarUrl = "/images/default-avatar.png",
                Role = "University Admin",
                UniName = admin.University.UniName ?? "Unknown University",
                TotalStudents = totalStudents,
                TotalAssessors = totalAssessors,
                TotalSupervisors = totalSupervisors,
                TotalSubjectCoordinators = totalSubjectCoordinators,
                TotalPrograms = totalPrograms,
                TotalCourses = totalCourses,
                TotalModules = totalModules
            };

            return View("~/Views/Dashboards/UA/Profile.cshtml", vm);
        }

        // Helper to fetch and map Staff data
        private async Task<List<StaffAndStudentVm.StaffRow>> GetStaffDataAsync(int uniId, string uniAbbrv)
        {
            var staffList = await _db.StaffProfiles
                .Where(s => s.User != null && s.User.UniID == uniId)
                .Include(s => s.User)
                .Include(s => s.StaffModules).ThenInclude(sm => sm.Module)
                .Select(s => new StaffAndStudentVm.StaffRow
                {
                    Id        = s.UserID ?? 0,
                    StaffID   = s.StaffID,
                    Name      = s.User != null ? (s.User.Name  ?? "") : "",
                    Email     = s.User != null ? (s.User.Email ?? "") : "",
                    UniAbbrv  = uniAbbrv,
                    Status    = s.User != null ? (s.User.Status ?? "") : "",
                    LastLogin = s.User != null ? s.User.LastLogin : null,
                    IsLocked  = s.User == null ? true : ((s.User.Status ?? "") != "Active"),
                    SessionNo = s.CurrentSessionID.HasValue ? s.CurrentSessionID.Value.ToString() : "N/A",

                    AssignedModules = s.StaffModules
                        .Select(sm => new StaffAndStudentVm.ModuleRow
                        {
                            ModuleCode = sm.Module != null ? sm.Module.ModuleCode : ""
                        })
                        .ToList()
                })
                .ToListAsync();

            return staffList;
        }

        // Helper to fetch and map Student data
        private async Task<List<StaffAndStudentVm.StudentRow>> GetStudentDataAsync(int uniId, string uniAbbrv)
        {
            var studentList = await _db.StudentProfiles
                .Where(s => s.Course != null && s.Course!.Program.UniID == uniId)
                .Include(s => s.User)
                .Include(s => s.Course!)
                    .ThenInclude(c => c.Program)
                .Include(s => s.StudentModules)
                    .ThenInclude(em => em.Module)
                .Select(s => new StaffAndStudentVm.StudentRow
                {
                    Id          = s.UserID ?? 0,
                    StudentID   = s.StudentID,
                    Name        = s.User != null ? (s.User.Name  ?? "") : "",
                    Email       = s.User != null ? (s.User.Email ?? "") : "",
                    UniAbbrv    = uniAbbrv,
                    Status      = s.User != null ? (s.User.Status ?? "") : "",
                    LastLogin   = s.User != null ? s.User.LastLogin : null,
                    IsLocked    = s.User == null ? true : ((s.User.Status ?? "") != "Active"),
                    ProgramName = s.Course != null ? s.Course.Program.ProgramName : "",
                    CourseName  = s.Course != null ? s.Course.CourseName : "",
                    SessionNo   = s.SessionID.HasValue ? s.SessionID.Value.ToString() : "N/A",
                    EnrolledModules = s.StudentModules
                        .Select(em => new StaffAndStudentVm.ModuleRow
                        {
                            ModuleCode = em.Module != null ? em.Module.ModuleCode : ""
                        })
                        .ToList()
                })
                .ToListAsync();

            return studentList;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string profileId, bool value)
        {
            if (string.IsNullOrWhiteSpace(profileId) || !int.TryParse(profileId, out int id))
            {
                TempData["Error"] = "Invalid profile ID submitted.";
                return RedirectToAction(nameof(UserMaster));
            }

            var userProfile = await _db.Users.FindAsync(id);
            if (userProfile == null)
            {
                TempData["Error"] = $"User with ID {id} not found.";
                return RedirectToAction(nameof(UserMaster));
            }

            userProfile.Status = value ? "Active" : "Locked";
            await _db.SaveChangesAsync();

            TempData["Flash"] = $"User '{userProfile.Name}' successfully {(value ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(UserMaster));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string profileId, string profileType)
        {
            if (string.IsNullOrWhiteSpace(profileId) || !int.TryParse(profileId, out int id))
            {
                TempData["Error"] = "Invalid profile ID submitted for password reset.";
                return RedirectToAction(nameof(UserMaster));
            }

            var userProfile = await _db.Users.FindAsync(id);
            if (userProfile == null)
            {
                TempData["Error"] = $"User with ID {id} not found.";
                return RedirectToAction(nameof(UserMaster));
            }

            // In a real application, you'd integrate an email service here.
            Toast($"Password reset initiated for {profileType} '{userProfile.Name}'. A temporary password has been emailed.", "info");
            return RedirectToAction(nameof(UserMaster));
        }
        
        // Removed original [HttpGet] public IActionResult Dashboard() => View();
        // Removed original [HttpGet] public IActionResult Users() { return RedirectToAction("Index", "UADashboard"); }

        // ====================================================================
        // CATALOG MANAGEMENT (from UniversityAdminController.cs)
        // ====================================================================

        // Catalog (DB-backed) 
        [HttpGet]
        public IActionResult Catalog()
        {
            // Programs
            var programs = _db.Programs
                .AsNoTracking()
                .OrderBy(p => p.ProgramCode)
                .Select(p => new ProgramVm { Code = p.ProgramCode, Name = p.ProgramName })
                .ToList();

            // Courses 
            var courses = _db.Courses
                .AsNoTracking()
                .Include(c => c.Program)
                .OrderBy(c => c.CourseCode)
                .Select(c => new CourseVm
                {
                    Code      = c.CourseCode,
                    Name      = c.CourseName,
                    ProgramCode = c.Program != null ? c.Program.ProgramCode : string.Empty,
                    CourseId    = c.ID
                })
                .ToList();

            // Modules 
            var modules = _db.Modules
                .AsNoTracking()
                .OrderBy(m => m.ModuleCode)
                .Select(m => new ModuleVm
                {
                    Code      = m.ModuleCode,
                    Name      = m.ModuleName,
                    ModuleId    = m.ID,
                    CourseCodes = (m.Courses != null
                        ? m.Courses.Select(c => c.CourseCode)
                        : Enumerable.Empty<string>())
                        .ToList()
                })
                .ToList();

            var model = new CatalogVm
            {
                Programs = programs,
                Courses  = courses,
                Modules  = modules
            };

            return View("~/Views/Dashboards/UA/Catalog.cshtml", model);
        }

        // Get counts from Catalog
        private async Task<(int programs, int courses, int modules)> GetCatalogCountsAsync(int uniId)
        {
            var programCount = await _db.Programs
                .CountAsync(p => p.UniID == uniId);

            var courseCount = await _db.Courses
                .Where(c => c.Program != null && c.Program.UniID == uniId)
                .CountAsync();

            var courseIds = await _db.Courses
                .Where(c => c.Program != null && c.Program.UniID == uniId)
                .Select(c => c.ID)
                .ToListAsync();

                    var moduleCount = await _db.Modules
                        .CountAsync(m => m.Courses.Any(c => courseIds.Contains(c.ID)));

                    return (programCount, courseCount, moduleCount);
        }

        // ----------------- PROGRAM CRUD -----------------
        [HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> CreateProgram(string ProgramCode, string ProgramName)
{
    ProgramCode = (ProgramCode ?? "").Trim().ToUpperInvariant();
    ProgramName = (ProgramName ?? "").Trim();

    StashForm("CreateProgram",
        ("ProgramCode", ProgramCode),
        ("ProgramName", ProgramName));

    bool hasErr = false;
    if (string.IsNullOrWhiteSpace(ProgramCode)) { FieldError("CreateProgram","ProgramCode","Program code is required."); hasErr = true; }
    if (string.IsNullOrWhiteSpace(ProgramName)) { FieldError("CreateProgram","ProgramName","Program name is required."); hasErr = true; }

    var uni = await _db.Universities.OrderBy(u => u.UniID).FirstOrDefaultAsync();
    if (uni == null) { Toast("No universities configured.", "danger"); return RedirectToAction(nameof(Catalog)); }

    var nameKey = ProgramName.ToUpperInvariant();

    if (!hasErr)
    {
        if (await _db.Programs.AnyAsync(p => p.UniID == uni.UniID && p.ProgramCode == ProgramCode))
            { FieldError("CreateProgram","ProgramCode", $"Program code {ProgramCode} is already in use."); hasErr = true; }

        if (await _db.Programs.AnyAsync(p => p.UniID == uni.UniID && p.ProgramName.ToUpper() == nameKey))
            { FieldError("CreateProgram","ProgramName", $"Program name '{ProgramName}' is already in use."); hasErr = true; }
    }

    if (hasErr) { Toast("Please fix the highlighted errors.", "danger", "mdlCreateProgram"); return RedirectToAction(nameof(Catalog)); }

    _db.Programs.Add(new DbProgram {
        UniID = uni.UniID,
        ProgramCode = ProgramCode,
        ProgramName = ProgramName
    });
    await _db.SaveChangesAsync();

    Toast($"Program {ProgramCode} created.", "success");
    return RedirectToAction(nameof(Catalog));
}


        // UpdateProgram
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgram(string ProgramCode, string ProgramName)
        {
            ProgramCode = (ProgramCode ?? "").Trim().ToUpperInvariant();
            ProgramName = (ProgramName ?? "").Trim();

            // keep user input
            StashForm("UpdateProgram",
                ("ProgramCode", ProgramCode),
                ("ProgramName", ProgramName));

            bool hasErr = false;
            if (string.IsNullOrWhiteSpace(ProgramCode)) { FieldError("UpdateProgram","ProgramCode","Program code is required."); hasErr = true; }
            if (string.IsNullOrWhiteSpace(ProgramName)) { FieldError("UpdateProgram","ProgramName","Program name is required."); hasErr = true; }

            var prg = await _db.Programs.FirstOrDefaultAsync(p => p.ProgramCode == ProgramCode);
            if (prg == null)
            {
                FieldError("UpdateProgram","ProgramCode",$"Program {ProgramCode} not found.");
                Toast("Please fix the highlighted errors.", "danger", "mdlUpdateProgram");
                return RedirectToAction(nameof(Catalog));
            }

            var nameKey = ProgramName.ToUpperInvariant();
            var dupName = await _db.Programs.AnyAsync(p => p.UniID == prg.UniID && p.ID != prg.ID && p.ProgramName.ToUpper() == nameKey);
            if (dupName)
            {
                FieldError("UpdateProgram","ProgramName", $"Program name '{ProgramName}' is already in use.");
                hasErr = true;
            }

            if (hasErr)
            {
                Toast("Please fix the highlighted errors.", "danger", "mdlUpdateProgram");
                return RedirectToAction(nameof(Catalog));
            }

            prg.ProgramName = ProgramName;
            await _db.SaveChangesAsync();

            Toast($"Program {ProgramCode} updated.", "success");
            return RedirectToAction(nameof(Catalog));
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProgram(string ProgramCode)
        {
            ProgramCode = (ProgramCode ?? "").Trim().ToUpperInvariant();
            var prg = await _db.Programs.FirstOrDefaultAsync(p => p.ProgramCode == ProgramCode);
            if (prg == null) { TempData["Toast"] = $"Program {ProgramCode} not found."; return RedirectToAction(nameof(Catalog)); }

            var hasCourses = await _db.Courses.AnyAsync(c => c.ProgramID == prg.ID);
            if (hasCourses)
            {
                TempData["Toast"] = "Cannot delete program: it has courses. Reassign or delete the courses first.";
                return RedirectToAction(nameof(Catalog));
            }

            _db.Programs.Remove(prg);
            await _db.SaveChangesAsync();
            TempData["Toast"] = $"Program {ProgramCode} deleted.";
            return RedirectToAction(nameof(Catalog));
        }

        // ---------- CREATE COURSE ----------
        [HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> CreateCourse(string CourseCode, string CourseName, string ProgramCode)
{
    CourseCode  = (CourseCode  ?? "").Trim().ToUpperInvariant();
    CourseName  = (CourseName  ?? "").Trim();
    ProgramCode = (ProgramCode ?? "").Trim().ToUpperInvariant();

    StashForm("CreateCourse",
        ("CourseCode", CourseCode),
        ("CourseName", CourseName),
        ("ProgramCode", ProgramCode));

    bool hasErr = false;
    if (string.IsNullOrWhiteSpace(CourseCode))  { FieldError("CreateCourse","CourseCode","Course code is required."); hasErr = true; }
    if (string.IsNullOrWhiteSpace(CourseName))  { FieldError("CreateCourse","CourseName","Course name is required."); hasErr = true; }
    if (string.IsNullOrWhiteSpace(ProgramCode)) { FieldError("CreateCourse","ProgramCode","Please choose a program."); hasErr = true; }
    if (hasErr) { Toast("Please fix the highlighted errors.","danger","mdlCreateCourse"); return RedirectToAction(nameof(Catalog)); }

    var prg = await _db.Programs.FirstOrDefaultAsync(p => p.ProgramCode == ProgramCode);
    if (prg == null) { FieldError("CreateCourse","ProgramCode",$"Program {ProgramCode} not found."); Toast("Please fix the highlighted errors.","danger","mdlCreateCourse"); return RedirectToAction(nameof(Catalog)); }

    var nameKey = CourseName.ToUpperInvariant();

    // Uniqueness inside the program
    bool dupCode = await _db.Courses.AnyAsync(c => c.ProgramID == prg.ID && c.CourseCode == CourseCode);
    bool dupName = await _db.Courses.AnyAsync(c => c.ProgramID == prg.ID && c.CourseName.ToUpper() == nameKey);

    if (dupCode) FieldError("CreateCourse","CourseCode",$"Course code {CourseCode} already exists in {ProgramCode}.");
    if (dupName) FieldError("CreateCourse","CourseName",$"Course name '{CourseName}' already exists in {ProgramCode}.");
    if (dupCode || dupName) { Toast("Please fix the highlighted errors.","danger","mdlCreateCourse"); return RedirectToAction(nameof(Catalog)); }

    _db.Courses.Add(new DbCourse {
        ProgramID  = prg.ID,
        CourseCode = CourseCode,
        CourseName = CourseName
    });
    await _db.SaveChangesAsync();

    Toast($"Course {CourseCode} created under {ProgramCode}.","success");
    return RedirectToAction(nameof(Catalog));
}



        // ---------- UPDATE COURSE ----------
        [HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateCourse(string CourseCode, string CourseName, string ProgramCode)
{
    CourseCode  = (CourseCode  ?? "").Trim().ToUpperInvariant();
    CourseName  = (CourseName  ?? "").Trim();
    ProgramCode = (ProgramCode ?? "").Trim().ToUpperInvariant();

    StashForm("UpdateCourse",
        ("CourseCode", CourseCode),
        ("CourseName", CourseName),
        ("ProgramCode", ProgramCode));

    bool hasErr = false;
    if (string.IsNullOrWhiteSpace(CourseCode))  { FieldError("UpdateCourse","CourseCode","Course code is required."); hasErr = true; }
    if (string.IsNullOrWhiteSpace(CourseName))  { FieldError("UpdateCourse","CourseName","Course name is required."); hasErr = true; }
    if (string.IsNullOrWhiteSpace(ProgramCode)) { FieldError("UpdateCourse","ProgramCode","Please choose a program."); hasErr = true; }
    if (hasErr) { Toast("Please fix the highlighted errors.","danger","mdlUpdateCourse"); return RedirectToAction(nameof(Catalog)); }

    var course = await _db.Courses.Include(c => c.Program).FirstOrDefaultAsync(c => c.CourseCode == CourseCode);
    if (course == null) { Toast($"Course {CourseCode} not found.","danger"); return RedirectToAction(nameof(Catalog)); }

    var newPrg = await _db.Programs.FirstOrDefaultAsync(p => p.ProgramCode == ProgramCode);
    if (newPrg == null) { FieldError("UpdateCourse","ProgramCode",$"Program {ProgramCode} not found."); Toast("Please fix the highlighted errors.","danger","mdlUpdateCourse"); return RedirectToAction(nameof(Catalog)); }

    var nameKey = CourseName.ToUpperInvariant();

    // Guard name uniqueness within target program (code stays same)
    bool dupName = await _db.Courses.AnyAsync(c =>
        c.ID != course.ID && c.ProgramID == newPrg.ID && c.CourseName.ToUpper() == nameKey);
    if (dupName) { FieldError("UpdateCourse","CourseName",$"Course name '{CourseName}' already exists in {ProgramCode}."); Toast("Please fix the highlighted errors.","danger","mdlUpdateCourse"); return RedirectToAction(nameof(Catalog)); }

    // Apply changes (allow moving course to a different program)
    course.ProgramID  = newPrg.ID;
    course.CourseName = CourseName;

    await _db.SaveChangesAsync();
    Toast($"Course {CourseCode} updated.","success");
    return RedirectToAction(nameof(Catalog));
}


        // ---------- DELETE COURSE----------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(string CourseCode)
        {
            CourseCode = (CourseCode ?? "").Trim().ToUpperInvariant();
            var course = await _db.Courses.Include(c => c.Modules).FirstOrDefaultAsync(c => c.CourseCode == CourseCode);
            if (course == null) { TempData["Toast"] = $"Course {CourseCode} not found."; return RedirectToAction(nameof(Catalog)); }

            await _db.Entry(course).Collection(c => c.Modules).LoadAsync();
            course.Modules ??= new List<DbModule>();
            course.Modules.Clear();

            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();
            TempData["Toast"] = $"Course {CourseCode} deleted.";
            return RedirectToAction(nameof(Catalog));
        }

        // ---------- CREATE MODULE ----------
        [HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> CreateModule(string ModuleCode, string ModuleName, List<string> CourseCodes)
{
    ModuleCode = (ModuleCode ?? "").Trim().ToUpperInvariant();
    ModuleName = (ModuleName ?? "").Trim();
    var codes = (CourseCodes ?? new())
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Select(s => s.Trim().ToUpperInvariant())
        .Distinct()
        .ToList();

    StashForm("CreateModule",
        ("ModuleCode", ModuleCode),
        ("ModuleName", ModuleName));

    bool hasErr = false;
    if (string.IsNullOrWhiteSpace(ModuleCode)) { FieldError("CreateModule","ModuleCode","Module code is required."); hasErr = true; }
    if (string.IsNullOrWhiteSpace(ModuleName)) { FieldError("CreateModule","ModuleName","Module name is required."); hasErr = true; }
    if (!codes.Any()) { FieldError("CreateModule","CourseCodes","Please select at least one course."); hasErr = true; }
    if (hasErr) { Toast("Please fix the highlighted errors.","danger","mdlCreateModule"); return RedirectToAction(nameof(Catalog)); }

    var nameKey = ModuleName.ToUpperInvariant();
    bool dupCode = await _db.Modules.AnyAsync(m => m.ModuleCode == ModuleCode);
    bool dupName = await _db.Modules.AnyAsync(m => m.ModuleName.ToUpper() == nameKey);
    if (dupCode) FieldError("CreateModule","ModuleCode",$"Module code {ModuleCode} already exists.");
    if (dupName) FieldError("CreateModule","ModuleName",$"Module name '{ModuleName}' already exists.");
    if (dupCode || dupName) { Toast("Please fix the highlighted errors.","danger","mdlCreateModule"); return RedirectToAction(nameof(Catalog)); }

    var courses = await _db.Courses.Where(c => codes.Contains(c.CourseCode)).ToListAsync();
    var missing = codes.Except(courses.Select(c => c.CourseCode), StringComparer.OrdinalIgnoreCase).ToList();
    if (missing.Any()) { FieldError("CreateModule","CourseCodes",$"Course code(s) not found: {string.Join(", ", missing)}"); Toast("Please fix the highlighted errors.","danger","mdlCreateModule"); return RedirectToAction(nameof(Catalog)); }

    var module = new DbModule { ModuleCode = ModuleCode, ModuleName = ModuleName };
    _db.Modules.Add(module);
    await _db.SaveChangesAsync();

    await _db.Entry(module).Collection(m => m.Courses).LoadAsync();
    module.Courses ??= new List<DbCourse>();
    foreach (var c in courses) module.Courses.Add(c);
    await _db.SaveChangesAsync();

    Toast($"Module {ModuleCode} created in {courses.Count} course(s).","success");
    return RedirectToAction(nameof(Catalog));
}


        // ---------- UPDATE MODULE ----------
        [HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> UpdateModule(string ModuleCode, string ModuleName, List<string> CourseCodes)
{
    ModuleCode = (ModuleCode ?? "").Trim().ToUpperInvariant();
    ModuleName = (ModuleName ?? "").Trim();
    var desiredCodes = new HashSet<string>((CourseCodes ?? new())
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Select(s => s.Trim().ToUpperInvariant()));

    StashForm("UpdateModule",
        ("ModuleCode", ModuleCode),
        ("ModuleName", ModuleName));

    bool hasErr = false;
    if (string.IsNullOrWhiteSpace(ModuleCode)) { FieldError("UpdateModule","ModuleCode","Module code is required."); hasErr = true; }
    if (string.IsNullOrWhiteSpace(ModuleName)) { FieldError("UpdateModule","ModuleName","Module name is required."); hasErr = true; }
    if (!desiredCodes.Any()) { FieldError("UpdateModule","CourseCodes","Please select at least one course."); hasErr = true; }
    if (hasErr) { Toast("Please fix the highlighted errors.","danger","mdlUpdateModule"); return RedirectToAction(nameof(Catalog)); }

    var module = await _db.Modules.Include(m => m.Courses).FirstOrDefaultAsync(m => m.ModuleCode == ModuleCode);
    if (module == null) { FieldError("UpdateModule","ModuleCode",$"Module {ModuleCode} not found."); Toast("Please fix the highlighted errors.","danger","mdlUpdateModule"); return RedirectToAction(nameof(Catalog)); }

    var nameKey = ModuleName.ToUpperInvariant();
    bool dupName = await _db.Modules.AnyAsync(m => m.ID != module.ID && m.ModuleName.ToUpper() == nameKey);
    if (dupName) { FieldError("UpdateModule","ModuleName",$"Module name '{ModuleName}' already exists."); Toast("Please fix the highlighted errors.","danger","mdlUpdateModule"); return RedirectToAction(nameof(Catalog)); }

    var courses = await _db.Courses.Where(c => desiredCodes.Contains(c.CourseCode)).ToListAsync();
    var missing = desiredCodes.Except(courses.Select(c => c.CourseCode), StringComparer.OrdinalIgnoreCase).ToList();
    if (missing.Any()) { FieldError("UpdateModule","CourseCodes",$"Course code(s) not found: {string.Join(", ", missing)}"); Toast("Please fix the highlighted errors.","danger","mdlUpdateModule"); return RedirectToAction(nameof(Catalog)); }

    module.ModuleName = ModuleName;

    await _db.Entry(module).Collection(m => m.Courses).LoadAsync();
    module.Courses ??= new List<DbCourse>();
    var desiredIds = courses.Select(c => c.ID).ToHashSet();
    var currentIds = module.Courses.Select(c => c.ID).ToHashSet();

    foreach (var c in module.Courses.Where(c => !desiredIds.Contains(c.ID)).ToList())
        module.Courses.Remove(c);
    foreach (var c in courses.Where(c => !currentIds.Contains(c.ID)))
        module.Courses.Add(c);

    await _db.SaveChangesAsync();
    Toast($"Module {ModuleCode} updated.","success");
    return RedirectToAction(nameof(Catalog));
}


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteModule(string ModuleCode)
        {
            ModuleCode = (ModuleCode ?? "").Trim().ToUpperInvariant();
            var module = await _db.Modules.Include(m => m.Courses).FirstOrDefaultAsync(m => m.ModuleCode == ModuleCode);
            if (module == null) { TempData["Toast"] = $"Module {ModuleCode} not found."; return RedirectToAction(nameof(Catalog)); }

            await _db.Entry(module).Collection(m => m.Courses).LoadAsync();
            module.Courses ??= new List<DbCourse>();
            module.Courses.Clear();

            _db.Modules.Remove(module);
            await _db.SaveChangesAsync();

            TempData["Toast"] = $"Module {ModuleCode} deleted.";
            return RedirectToAction(nameof(Catalog));
        }

        // ====================================================================
        // UNIVERSITY CONSTRAINTS (from UniversityAdminController.cs)
        // ====================================================================

        // University constraints (DB-backed)
        [HttpGet]
        public IActionResult University(int? uniId, int? year = null)
        {
            var university = uniId.HasValue && uniId.Value > 0
                ? _db.Universities.FirstOrDefault(u => u.UniID == uniId.Value)
                : _db.Universities.OrderBy(u => u.UniID).FirstOrDefault();

            if (university == null) return NotFound("No universities configured.");

            int theYear = year ?? DateTime.UtcNow.Year;

            var constraints = _db.UniversityConstraints
                                 .FirstOrDefault(c => c.UniID == university.UniID && c.Year == theYear);

            var vm = new UniversityConstraintsVm
            {
                UniCode = university.UniID.ToString(),
                UniversityName = university.UniName,
                Abbrev = university.UniAbbrv,

                Year = constraints?.Year ?? theYear,
                SupervisorLoadCap = constraints?.SLoadCap ?? 0,
                AssessorLoadCap = constraints?.ALoadCap ?? 0,
                PreferencesRankLimit = constraints?.PrefRankLimit ?? 0,
                NumSessions = constraints?.SessionNo ?? 0,
                TeamSizeMin = constraints?.SMin ?? 0,
                TeamSizeMax = constraints?.SMax ?? 0,

                S1Label = constraints?.S1Label ?? "S1",
                S2Label = constraints?.S2Label ?? "S2",
                S3Label = constraints?.S3Label ?? "S3",
                S4Label = constraints?.S4Label ?? "S4",

                S1Range = RangeToString(constraints?.S1_Dte_fr, constraints?.S1_Dte_to),
                S2Range = RangeToString(constraints?.S2_Dte_fr, constraints?.S2_Dte_to),
                S3Range = RangeToString(constraints?.S3_Dte_fr, constraints?.S3_Dte_to),
                S4Range = RangeToString(constraints?.S4_Dte_fr, constraints?.S4_Dte_to),
            };

            ViewBag.Universities = _db.Universities
                .OrderBy(u => u.UniName)
                .Select(u => new { u.UniID, u.UniName })
                .ToList();

            var baseYear = DateTime.UtcNow.Year - 1;
            vm.Years = Enumerable.Range(baseYear, 7).ToList();

            return View("~/Views/Dashboards/UA/University.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveConstraints([FromBody] UniversityConstraintsVm vm)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid payload");
            if (!int.TryParse(vm.UniCode, out int uniId))
                return BadRequest("Invalid UniCode");

            var c = _db.UniversityConstraints.FirstOrDefault(x => x.UniID == uniId && x.Year == vm.Year);
            if (c == null)
            {
                c = new UniversityConstraint { UniID = uniId, Year = vm.Year };
                _db.UniversityConstraints.Add(c);
            }

            c.SLoadCap = vm.SupervisorLoadCap;
            c.ALoadCap = vm.AssessorLoadCap;
            c.PrefRankLimit = vm.PreferencesRankLimit;
            c.SessionNo = vm.NumSessions;
            c.SMin = vm.TeamSizeMin;
            c.SMax = vm.TeamSizeMax;

            c.S1Label = vm.S1Label;
            c.S2Label = vm.S2Label;
            c.S3Label = vm.S3Label;
            c.S4Label = vm.S4Label;

            (c.S1_Dte_fr, c.S1_Dte_to) = ParseRange(vm.S1Range);
            (c.S2_Dte_fr, c.S2_Dte_to) = ParseRange(vm.S2Range);
            (c.S3_Dte_fr, c.S3_Dte_to) = ParseRange(vm.S3Range);
            (c.S4_Dte_fr, c.S4_Dte_to) = ParseRange(vm.S4Range);

            c.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            return Ok(new { ok = true, message = "Saved" });
        }


        // ====================================================================
        // CSV UPLOAD & TEMPLATE (from UniversityAdminController.cs)
        // ====================================================================

        // Template CSV 
        [HttpGet]
        public IActionResult TemplateCsv(string kind = "programs")
        {
            kind = (kind ?? "").Trim().ToLowerInvariant();

            string fileName;
            string csv;

            switch (kind)
            {
                case "programs":
                    fileName = "programs.csv";
                    csv = "ProgramCode,ProgramName\n";
                    break;
                case "courses":
                    fileName = "courses.csv";
                    csv = "ProgramCode,CourseCode,CourseName\n";
                    break;
                case "modules":
                    fileName = "modules.csv";
                    csv = "CourseCode,ModuleCode,ModuleName\n";
                    break;
                default:
                    fileName = "programs.csv";
                    csv = "ProgramID,ProgramCode,ProgramName\n";
                    break;
            }

            var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv);
            return File(bytes, "text/csv", fileName);
        }

        // UploadCatalogCsv (FULL LOGIC)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCatalogCsv(List<IFormFile> files)
        {
            var warnings = new List<string>();
            var errors = new List<string>();
            if (files == null || files.Count == 0)
                return BadRequest(new { ok = false, errors = new[] { "No files selected." } });

            var uni = await _db.Universities.OrderBy(u => u.UniID).FirstOrDefaultAsync();
            if (uni == null) return BadRequest(new { ok = false, errors = new[] { "No universities configured." } });
            int uniId = uni.UniID;

            int pIns = 0, pUpd = 0, cIns = 0, cUpd = 0, mIns = 0, mUpd = 0;

            // pass 0: classify files
            var progFiles   = new List<IFormFile>();
            var courseFiles = new List<IFormFile>();
            var moduleFiles = new List<IFormFile>();

            foreach (var f in files)
            {
                if (f == null || f.Length == 0) { errors.Add($"{(f?.FileName ?? "(unknown file)")}: empty file."); continue; }
                using var sr = new StreamReader(f.OpenReadStream(), Encoding.UTF8, true);
                var first = await sr.ReadLineAsync() ?? "";
                var hdr = SplitCsvLine(first);

                if (HeaderIs(hdr, "ProgramCode", "ProgramName")) progFiles.Add(f);
                else if (HeaderIs(hdr, "ProgramCode", "CourseCode", "CourseName")) courseFiles.Add(f);
                else if (HeaderIs(hdr, "CourseCode", "ModuleCode", "ModuleName"))moduleFiles.Add(f);
                else errors.Add($"{(f?.FileName ?? "(unknown file)")}: unknown template header.");
            }
            if (errors.Count > 0) return BadRequest(new { ok = false, errors });

            await using var tx = await _db.Database.BeginTransactionAsync();

            // ===== pass 1: PROGRAMS (no ProgramID) =====
foreach (var file in progFiles)
{
    if (file == null) { errors.Add("(unknown file): null file handle."); continue; }
    var fname = file.FileName ?? "(unknown file)";

    using var sr = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true);
    await sr.ReadLineAsync(); // header
    int lineNo = 1;

    while (!sr.EndOfStream)
    {
        var line = await sr.ReadLineAsync(); lineNo++;
        if (string.IsNullOrWhiteSpace(line)) continue;

        var cols = SplitCsvLine(line);
        Require(cols, 2, fname, lineNo, errors); if (cols.Length < 2) continue;

        var prgCode = MustValue(cols[0], "ProgramCode", fname, lineNo, errors).ToUpperInvariant();
        var prgName = MustValue(cols[1], "ProgramName", fname, lineNo, errors);
        if (string.IsNullOrWhiteSpace(prgCode) || string.IsNullOrWhiteSpace(prgName)) continue;

        var nameKey = prgName.Trim().ToUpperInvariant();

        // Look up by business keys within same uni
        var byCode = await _db.Programs.FirstOrDefaultAsync(x => x.UniID == uniId && x.ProgramCode == prgCode);
        var byName = await _db.Programs.FirstOrDefaultAsync(x => x.UniID == uniId && x.ProgramName.ToUpper() == nameKey);

        // If both keys exist but to different rows -> conflict
        var found = new[] { byCode, byName }.Where(x => x != null).ToList();
        var distinctIds = found.Select(x => x!.ID).Distinct().ToList();
        if (distinctIds.Count > 1)
        {
            var details = new List<string>();
            if (byCode != null) details.Add($"ProgramCode '{prgCode}' → [Code={byCode.ProgramCode}, Name={byCode.ProgramName}]");
            if (byName != null) details.Add($"ProgramName '{prgName}' → [Code={byName.ProgramCode}, Name={byName.ProgramName}]");
            errors.Add($"{fname} line {lineNo}: keys point to DIFFERENT existing programs: {string.Join("; ", details)}.");
            continue;
        }

        var existing = byCode ?? byName;
        if (existing != null)
        {
            // guard uniqueness when renaming
            var dupCode = await _db.Programs.AnyAsync(p => p.UniID == uniId && p.ID != existing.ID && p.ProgramCode == prgCode);
            var dupName = await _db.Programs.AnyAsync(p => p.UniID == uniId && p.ID != existing.ID && p.ProgramName.ToUpper() == nameKey);
            if (dupCode) { errors.Add($"{fname} line {lineNo}: ProgramCode '{prgCode}' already used by another program."); continue; }
            if (dupName) { errors.Add($"{fname} line {lineNo}: ProgramName '{prgName}' already used by another program."); continue; }

            existing.ProgramCode = prgCode;
            existing.ProgramName = prgName;
            pUpd++;
            continue;
        }

        // Insert new
        // ensure code/name unique in uni
        var codeOwner = await _db.Programs.FirstOrDefaultAsync(x => x.UniID == uniId && x.ProgramCode == prgCode);
        if (codeOwner != null) { errors.Add($"{fname} line {lineNo}: ProgramCode '{prgCode}' already exists."); continue; }
        var nameOwner = await _db.Programs.FirstOrDefaultAsync(x => x.UniID == uniId && x.ProgramName.ToUpper() == nameKey);
        if (nameOwner != null) { errors.Add($"{fname} line {lineNo}: ProgramName '{prgName}' already exists."); continue; }

        _db.Programs.Add(new DbProgram {
            UniID = uniId,
            ProgramCode = prgCode,
            ProgramName = prgName
        });
        pIns++;
    }
}
if (errors.Count > 0) return BadRequest(new { ok = false, errors });



            // ===== pass 2: COURSES ----------
            
foreach (var file in courseFiles)
{
    if (file == null) { errors.Add("(unknown file): null file handle."); continue; }
    var fname = file.FileName ?? "(unknown file)";
    using var sr = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true);
    await sr.ReadLineAsync(); // skip header
    int lineNo = 1;

    while (!sr.EndOfStream)
    {
        var line = await sr.ReadLineAsync(); lineNo++;
        if (string.IsNullOrWhiteSpace(line)) continue;

        var cols = SplitCsvLine(line);
        Require(cols, 3, fname, lineNo, errors); if (cols.Length < 3) continue;

        var programCd = MustValue(cols[0], "ProgramCode", fname, lineNo, errors).ToUpperInvariant();
        var courseCd  = MustValue(cols[1], "CourseCode",  fname, lineNo, errors).ToUpperInvariant();
        var courseNm  = MustValue(cols[2], "CourseName",  fname, lineNo, errors);
        if (string.IsNullOrWhiteSpace(programCd) || string.IsNullOrWhiteSpace(courseCd) || string.IsNullOrWhiteSpace(courseNm))
            continue;

        var nameKey = courseNm.Trim().ToUpperInvariant();

        // validate Program exists
        var program = await _db.Programs.FirstOrDefaultAsync(x => x.UniID == uniId && x.ProgramCode == programCd);
        if (program == null)
        {
            errors.Add($"{fname} line {lineNo}: ProgramCode '{programCd}' not found.");
            continue;
        }

        // find existing by course code or name within same program
        var byCode = await _db.Courses.Include(c => c.Program)
            .FirstOrDefaultAsync(c => c.ProgramID == program.ID && c.CourseCode == courseCd);
        var byName = await _db.Courses.Include(c => c.Program)
            .FirstOrDefaultAsync(c => c.ProgramID == program.ID && c.CourseName.ToUpper() == nameKey);

        var found = new[] { byCode, byName }.Where(x => x != null).ToList();
        var distinctIds = found.Select(x => x!.ID).Distinct().ToList();

        if (distinctIds.Count > 1)
        {
            errors.Add($"{fname} line {lineNo}: CourseCode '{courseCd}' and CourseName '{courseNm}' refer to different courses in program '{programCd}'.");
            continue;
        }

        var existing = byCode ?? byName;
        if (existing != null)
        {
            // Update name if needed
            existing.CourseCode = courseCd;
            existing.CourseName = courseNm;
            cUpd++;
            continue;
        }

        // Insert new course
        var dupCode = await _db.Courses.AnyAsync(c => c.ProgramID == program.ID && c.CourseCode == courseCd);
        var dupName = await _db.Courses.AnyAsync(c => c.ProgramID == program.ID && c.CourseName.ToUpper() == nameKey);
        if (dupCode || dupName)
        {
            errors.Add($"{fname} line {lineNo}: Duplicate CourseCode or CourseName in program '{programCd}'.");
            continue;
        }

        _db.Courses.Add(new DbCourse
        {
            ProgramID  = program.ID,
            CourseCode = courseCd,
            CourseName = courseNm
        });
        cIns++;
    }
}
if (errors.Count > 0) return BadRequest(new { ok = false, errors });



            // ===== pass 3: MODULES (new) =====
          // ===== pass 3: MODULES (no ModuleID) =====
foreach (var file in moduleFiles)
{
    if (file == null) { errors.Add("(unknown file): null file handle."); continue; }
    var fname = file.FileName ?? "(unknown file)";
    using var sr = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true);

    var header = SplitCsvLine(await sr.ReadLineAsync() ?? "");
    bool newFormat    = HeaderIs(header, "ModuleCode", "ModuleName", "CourseCodes");       // new: 3 cols
    bool legacyFormat = HeaderIs(header, "CourseCode", "ModuleCode", "ModuleName");        // legacy: 3 cols

    if (!newFormat && !legacyFormat)
    {
        errors.Add($"{fname}: unknown module template header.");
        continue;
    }

    int lineNo = 1;

    var seenCsvModCode = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var seenCsvModName = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    while (!sr.EndOfStream)
    {
        var line = await sr.ReadLineAsync(); lineNo++;
        if (string.IsNullOrWhiteSpace(line)) continue;

        var cols = SplitCsvLine(line);
        Require(cols, 3, fname, lineNo, errors);
        if (cols.Length < 3) continue;

        string moduleCd, moduleNm;
        List<string> courseCodes;

        if (newFormat)
        {
            // ModuleCode, ModuleName, CourseCodes (semicolon-separated)
            moduleCd = MustValue(cols[0], "ModuleCode", fname, lineNo, errors).ToUpperInvariant();
            moduleNm = MustValue(cols[1], "ModuleName", fname, lineNo, errors);
            var ccStr = MustValue(cols[2], "CourseCodes", fname, lineNo, errors);
            courseCodes = ccStr
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToUpperInvariant())
                .Distinct()
                .ToList();
        }
        else // legacyFormat: CourseCode, ModuleCode, ModuleName  (single course)
        {
            var legacyCourse = MustValue(cols[0], "CourseCode", fname, lineNo, errors).ToUpperInvariant();
            moduleCd = MustValue(cols[1], "ModuleCode", fname, lineNo, errors).ToUpperInvariant();
            moduleNm = MustValue(cols[2], "ModuleName", fname, lineNo, errors);
            courseCodes = new List<string> { legacyCourse };
        }

        var nameKey = moduleNm.Trim().ToUpperInvariant();

        if (!seenCsvModCode.Add(moduleCd)) { errors.Add($"{fname} line {lineNo}: duplicate ModuleCode '{moduleCd}' in uploaded files."); continue; }
        if (!seenCsvModName.Add(nameKey))  { errors.Add($"{fname} line {lineNo}: duplicate ModuleName '{moduleNm}' in uploaded files."); continue; }

        // Look up existing (global by business keys)
        var byCode = await _db.Modules.FirstOrDefaultAsync(m => m.ModuleCode == moduleCd);
        var byName = await _db.Modules.FirstOrDefaultAsync(m => m.ModuleName.ToUpper() == nameKey);

        var matches = new[] { byCode, byName }.Where(x => x != null).Select(x => x!.ID).Distinct().ToList();
        if (matches.Count > 1)
        {
            errors.Add($"{fname} line {lineNo}: ModuleCode '{moduleCd}' and ModuleName '{moduleNm}' refer to different modules.");
            continue;
        }

        DbModule? module = byCode ?? byName;

        if (module != null)
        {
            bool codeClash = await _db.Modules.AnyAsync(m => m.ID != module.ID && m.ModuleCode == moduleCd);
            bool nameClash = await _db.Modules.AnyAsync(m => m.ID != module.ID && m.ModuleName.ToUpper() == nameKey);
            if (codeClash) { errors.Add($"{fname} line {lineNo}: ModuleCode '{moduleCd}' already used by another module."); continue; }
            if (nameClash) { errors.Add($"{fname} line {lineNo}: ModuleName '{moduleNm}' already used by another module."); continue; }

            module.ModuleCode = moduleCd;
            module.ModuleName = moduleNm;
            mUpd++;
        }
        else
        {
            bool dupCode = await _db.Modules.AnyAsync(m => m.ModuleCode == moduleCd);
            bool dupName = await _db.Modules.AnyAsync(m => m.ModuleName.ToUpper() == nameKey);
            if (dupCode || dupName)
            {
                errors.Add($"{fname} line {lineNo}: Duplicate ModuleCode or ModuleName.");
                continue;
            }

            module = new DbModule { ModuleCode = moduleCd, ModuleName = moduleNm };
            _db.Modules.Add(module);
            mIns++;
        }

        // Sync many-to-many
        await _db.Entry(module).Collection(m => m.Courses).LoadAsync();
        module.Courses ??= new List<DbCourse>();

        var courses = await _db.Courses.Where(c => courseCodes.Contains(c.CourseCode)).ToListAsync();
        var notFound = courseCodes.Except(courses.Select(c => c.CourseCode), StringComparer.OrdinalIgnoreCase).ToList();
        if (notFound.Any()) { errors.Add($"{fname} line {lineNo}: CourseCode(s) not found: {string.Join(", ", notFound)}"); continue; }

        var desiredIds = courses.Select(c => c.ID).ToHashSet();
        var currentIds = module.Courses.Select(c => c.ID).ToHashSet();

        foreach (var c in module.Courses.Where(c => !desiredIds.Contains(c.ID)).ToList())
            module.Courses.Remove(c);
        foreach (var c in courses.Where(c => !currentIds.Contains(c.ID)))
            module.Courses.Add(c);
    }
}
if (errors.Count > 0) return BadRequest(new { ok = false, errors });


// If we got here, all 3 passes parsed OK. Persist & finish the transaction.
try
{
    await _db.SaveChangesAsync();
    await tx.CommitAsync();
}
catch (DbUpdateException dbx)
{
    await tx.RollbackAsync();
    var msg = dbx.InnerException?.Message ?? dbx.Message;
    if (msg.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase))
        msg = "Duplicate detected by the database. Check for repeated Codes/Names.";
    return BadRequest(new { ok = false, errors = new[] { msg } });
}

// Return summary counts so the UI can toast something nice 🙂
return Ok(new
{
    ok = true,
    summary = new
    {
        programsInserted = pIns,
        programsUpdated  = pUpd,
        coursesInserted  = cIns,
        coursesUpdated   = cUpd,
        modulesInserted  = mIns,
        modulesUpdated   = mUpd
    }
});


        }


        // ====================================================================
        // HELPER METHODS (from UniversityAdminController.cs)
        // ====================================================================

        private void Toast(string message, string level = "info", string? reopenModalId = null)
        {
            TempData["Toast"] = message;
            TempData["ToastLevel"] = level;          // "success" | "warning" | "danger" | "info"
            if (!string.IsNullOrWhiteSpace(reopenModalId))
                TempData["ReopenModal"] = reopenModalId;
        }

        private void FieldError(string prefix, string field, string message)
        {
            TempData[$"Err.{prefix}.{field}"] = message;
        }

        private void StashForm(string prefix, params (string key, string? val)[] fields)
        {
            foreach (var (key,val) in fields)
                TempData[$"Form.{prefix}.{key}"] = val ?? "";
        }

        private void FieldError(string prefix, string field, string message, string reopenModalId)
        {
            TempData[$"Err.{prefix}.{field}"] = message;
            TempData["ReopenModal"] = reopenModalId;   // auto-reopen the modal after redirect
        }

        // CSV helpers 
        private static bool HeaderIs(string[] hdr, params string[] expected)
        {
            if (hdr.Length < expected.Length) return false;
            for (int i = 0; i < expected.Length; i++)
                if (!string.Equals(hdr[i].Trim(), expected[i], StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }
        private static void Require(string[] cols, int count, string file, int line, List<string> errors)
        {
            if (cols.Length < count)
                errors.Add($"{file} line {line}: expected {count} columns, got {cols.Length}.");
        }
        private static int ParseIntRequired(string? v, string name, string file, int line, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(v)) { errors.Add($"{file} line {line}: {name} is required."); return 0; }
            if (!int.TryParse(v.Trim(), out var n)) { errors.Add($"{file} line {line}: {name} must be an integer."); return 0; }
            return n;
        }
        private static string MustValue(string? v, string name, string file, int line, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(v)) { errors.Add($"{file} line {line}: {name} is required."); return ""; }
            return v.Trim();
        }
        private static string[] SplitCsvLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return Array.Empty<string>();
            var res = new List<string>();
            var sb = new StringBuilder();
            bool inQ = false;
            for (int i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    if (inQ && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQ = !inQ;
                }
                else if (ch == ',' && !inQ)
                {
                    res.Add(sb.ToString());
                    sb.Clear();
                }
                else sb.Append(ch);
            }
            res.Add(sb.ToString());
            return res.ToArray();
        }

        private static string RangeToString(DateTime? fr, DateTime? to)
            => (fr == null || to == null) ? "" : $"{fr.Value:dd MMM yyyy} - {to.Value:dd MMM yyyy}";

        private static (DateTime?, DateTime?) ParseRange(string? range)
        {
            if (string.IsNullOrWhiteSpace(range) || !range.Contains('-')) return (null, null);
            var parts = range.Split('-', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2) return (null, null);

            var formats = new[] { "dd MMM yyyy", "d MMM yyyy", "yyyy-MM-dd" };
            var style = DateTimeStyles.AllowWhiteSpaces;

            DateTime? fr = DateTime.TryParseExact(parts[0], formats, CultureInfo.InvariantCulture, style, out var d1) ? d1 : null;
            DateTime? to = DateTime.TryParseExact(parts[1], formats, CultureInfo.InvariantCulture, style, out var d2) ? d2 : null;

            return (fr, to);
        }
        private static string MakeAbbrev(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var letters = string.Concat(parts.Select(p => char.ToUpperInvariant(p[0])));
                return letters.Length <= 10 ? letters : letters[..10];
            }
            return name.Length <= 10 ? name : name[..10];
        }
    }
}