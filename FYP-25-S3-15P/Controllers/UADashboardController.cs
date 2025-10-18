using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using FYP_25_S3_15P.Services;
using FYP_25_S3_15P.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // IPasswordHasher<User>
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FYP_25_S3_15P.Controllers
{
    [Authorize(Roles = "University Admin")]
    public class UADashboardController : Controller
    {
        private readonly SmartDbContext _db;

        public UADashboardController(SmartDbContext db) => _db = db;

        // Helper to get current user's university ID
        private async Task<string?> GetCurrentUserUniIDAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out var userId))
            {
                return await _db.Users
                    .Where(u => u.ID == userId)
                    .Select(u => u.UniID)
                    .FirstOrDefaultAsync();
            }
            return null;
        }

        // Helper to get current user ID (synchronous version)
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // ============== DASHBOARD ==============
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var vm = new UADashboardVm
            {
                AdminName = User.Identity?.Name ?? "Admin",
                UniversityName = await _db.Universities
                    .Where(u => u.UniID == uniID)
                    .Select(u => u.UniName)
                    .FirstOrDefaultAsync() ?? "University",
                TotalUsers = await _db.Users.CountAsync(u => u.UniID == uniID),
                TotalPrograms = await _db.Programs.CountAsync(p => p.UniID == uniID),
                TotalCourses = await _db.Courses
                    .Include(c => c.Programs)
                    .Where(c => c.Programs != null && c.Programs.UniID == uniID)
                    .CountAsync(),
                TotalExperts = await _db.StaffProfiles
                    .Include(s => s.User)
                    .Where(s => s.User != null && s.User.UniID == uniID)
                    .CountAsync()
            };

            return View("~/Views/Dashboards/UA/Index.cshtml", vm);
        }

        // ============== MANAGE PROFILE ==============
        [HttpGet]
        public IActionResult ViewProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                return RedirectToAction("AccessDenied", "Account");

            return RedirectToAction(nameof(ManageProfile), new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageProfile(int id)
        {
            var currentUserId = GetCurrentUserId();

            // Only allow users to view their own profile via this action
            if (currentUserId != id)
                return RedirectToAction("AccessDenied", "Account");

            var user = await _db.Users
                .Include(u => u.Role)
                .Include(u => u.University)
                .Where(u => u.ID == id)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            var vm = new UserDetailVm
            {
                ID = user.ID,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                UniID = user.UniID ?? string.Empty,
                UniversityName = user.University?.UniName ?? "-",
                Role = user.Role?.Name ?? "-",
                RoleID = user.RoleID,
                Status = user.Status ?? "-",
                IsLocked = user.IsLocked,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                IsOwnProfile = true
            };

            return View("~/Views/Dashboards/UA/ManageProfile.cshtml", vm);
        }

        [HttpGet]
        public IActionResult UpdateProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                return RedirectToAction("AccessDenied", "Account");

            return RedirectToAction(nameof(ManageProfile), new { id = userId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int userId, string currentPassword, string newPassword, string confirmPassword)
        {
            var currentUserId = GetCurrentUserId();

            // Only allow users to change their own password
            if (currentUserId != userId)
                return RedirectToAction("AccessDenied", "Account");

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                return RedirectToAction(nameof(ManageProfile), new { id = userId });
            }

            var user = await _db.Users
                .Where(u => u.ID == userId)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            // Verify current password (TODO: replace with proper hash verification)
            if (user.Password != currentPassword)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction(nameof(ManageProfile), new { id = userId });
            }

            // Update password (TODO: Hash this properly)
            user.Password = newPassword;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Password changed successfully!";
            return RedirectToAction(nameof(ManageProfile), new { id = userId });
        }

        // ============== MANAGE USERS ==============
        [HttpGet]
        public async Task<IActionResult> ManageUsersList(string? search = null)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var query = _db.Users
                .Include(u => u.Role)
                .Include(u => u.University)
                .Where(u => u.UniID == uniID);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u =>
                    (u.Name != null && u.Name.ToLower().Contains(searchLower)) ||
                    (u.Email != null && u.Email.ToLower().Contains(searchLower)) ||
                    (u.University != null && u.University.UniName != null && u.University.UniName.ToLower().Contains(searchLower)) ||
                    (u.Role != null && u.Role.Name != null && u.Role.Name.ToLower().Contains(searchLower)) ||
                    (u.Status != null && u.Status.ToLower().Contains(searchLower)));
            }

            var users = await query
                .Select(u => new UserMasterVm.Row
                {
                    ID = u.ID,
                    Name = u.Name ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    UniName = u.University != null ? u.University.UniName ?? "-" : "-",
                    RoleID = u.RoleID,
                    Role = u.Role != null ? u.Role.Name ?? "-" : "-",
                    Status = u.Status ?? "-",
                    IsLocked = u.IsLocked,
                    LastLogin = u.LastLogin
                })
                .ToListAsync();

            var vm = new UserMasterVm { Users = users };
            ViewBag.SearchQuery = search ?? string.Empty;

            return View("~/Views/Dashboards/UA/ManageUsersList.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> ViewUserProfile(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var currentUserId = GetCurrentUserId();

            // Prevent viewing own profile from this action
            if (currentUserId == id)
                return RedirectToAction(nameof(ManageProfile), new { id });

            var user = await _db.Users
                .Include(u => u.Role)
                .Include(u => u.University)
                .Where(u => u.ID == id && u.UniID == uniID)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            // Get the creator's name separately if CreatedBy is not empty
            string createdByName = "System";
            if (!string.IsNullOrEmpty(user.CreatedBy) && int.TryParse(user.CreatedBy, out int creatorId))
            {
                createdByName = await _db.Users
                    .Where(u => u.ID == creatorId)
                    .Select(u => u.Name)
                    .FirstOrDefaultAsync() ?? user.CreatedBy;
            }

            var vm = new UserDetailVm
            {
                ID = user.ID,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                UniversityName = user.University?.UniName ?? "-",
                Role = user.Role?.Name ?? "-",
                RoleID = user.RoleID,
                Status = user.Status ?? "-",
                IsLocked = user.IsLocked,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                CreatedBy = createdByName,
                IsOwnProfile = true
            };

            return View("~/Views/Dashboards/UA/ViewUserProfile.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUserAccount()
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var vm = new CreateEditUserVm();
            ViewBag.Roles = await _db.Roles.ToListAsync();
            return View("~/Views/Dashboards/UA/CreateUserAccount.cshtml", new User());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserAccount(CreateEditUserVm model)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            model.UniID = uniID;

            if (!ModelState.IsValid)
            {
                model.Roles = await _db.Roles
                    .Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name })
                    .ToListAsync();
                return View("~/Views/Dashboards/UA/CreateUserAccount.cshtml", model);
            }

            var currentUserId = GetCurrentUserId();

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password ?? string.Empty,
                RoleID = model.RoleID,
                UniID = uniID,
                Status = model.Status,
                IsLocked = model.IsLocked,
                MustChangePassword = model.MustChangePassword,
                CreatedBy = currentUserId.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            // TODO: Hash password properly

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "User created successfully!";
            return RedirectToAction(nameof(ManageUsersList));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateUserAccount(int id)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId != id)
                return RedirectToAction("AccessDenied", "Account");

            var user = await _db.Users
                .Include(u => u.Role)
                .Include(u => u.University)
                .Where(u => u.ID == id)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            var vm = new CreateEditUserVm
            {
                ID = user.ID,
                Name = user.Name,
                Email = user.Email,
                RoleID = user.RoleID,
                UniID = user.UniID,
                Status = user.Status,
                IsLocked = user.IsLocked,
                MustChangePassword = user.MustChangePassword
            };
        
            ViewBag.Roles = await _db.Roles.ToListAsync();
            return View("~/Views/Dashboards/UA/UpdateUserAccount.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserAccount(CreateEditUserVm model)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId != model.ID)
                return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
                return View("~/Views/Dashboards/UA/UpdateUserAccountProfile.cshtml", model);

            var user = await _db.Users
                .Where(u => u.ID == model.ID)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Email = model.Email;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "User updated successfully!";
            return RedirectToAction(nameof(ViewUserProfile), new { id = user.ID });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableUserAccount(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var user = await _db.Users
                .Where(u => u.ID == id && u.UniID == uniID)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            user.IsLocked = true;
            user.Status = "Disabled";
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "User disabled successfully!";
            return RedirectToAction(nameof(ManageUsersList));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateUserAccount(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var user = await _db.Users
                .Where(u => u.ID == id && u.UniID == uniID)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            user.IsLocked = false;
            user.Status = "Active";
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "User reactivated successfully!";
            return RedirectToAction(nameof(ViewUserProfile), new { id = user.ID });
        }

        // ============== MANAGE PROGRAMS ==============
        [HttpGet]
        public async Task<IActionResult> ManageProgram(string? search = null)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var query = _db.Programs
                .Include(p => p.Course)
                .Where(p => p.UniID == uniID);

            // Add search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    (p.ProgramID != null && p.ProgramID.ToLower().Contains(searchLower)) ||
                    (p.ProgramName != null && p.ProgramName.ToLower().Contains(searchLower)));
            }

            var programs = await query
                .Select(p => new ProgramMasterVm.ProgramRow
                {
                    ID = p.ID,
                    ProgramID = p.ProgramID ?? "-",
                    ProgramName = p.ProgramName ?? "-",
                    CourseCount = _db.Courses.Count(c => c.ProgramID == p.ProgramID)
                })
                .ToListAsync();

            var vm = new ProgramMasterVm { Programs = programs };
            ViewBag.SearchQuery = search ?? string.Empty;

            return View("~/Views/Dashboards/UA/ManageProgram.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> ViewProgram(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var program = await _db.Programs
                .Include(p => p.University)
                .Where(p => p.ID == id && p.UniID == uniID)
                .FirstOrDefaultAsync();

            if (program == null)
                return NotFound();

            var vm = new ProgramDetailVm
            {
                ID = program.ID,
                ProgramID = program.ProgramID ?? string.Empty,
                ProgramName = program.ProgramName ?? string.Empty,
                UniID = program.UniID ?? string.Empty,
                UniversityName = program.University?.UniName ?? "-"
            };

            return View("~/Views/Dashboards/UA/ViewProgram.cshtml", vm);
        }

        [HttpGet]
        public IActionResult CreateProgram()
        {
            return View("~/Views/Dashboards/UA/CreateProgram.cshtml", new CreateEditProgramVm());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProgram(CreateEditProgramVm model)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            ModelState.Remove("ProgramID");
            ModelState.Remove("UniID");

            model.UniID = uniID;

            // Auto - generate ProgramID if not provided
            if (string.IsNullOrEmpty(model.ProgramID))
            {
                model.ProgramID = $"PROG_{uniID}_{DateTime.UtcNow.Ticks}_{Guid.NewGuid().ToString().Substring(0, 6)}";
            }

            if (!ModelState.IsValid)
                return View("~/Views/Dashboards/UA/CreateProgram.cshtml", model);


            var program = new Programs
            {
                ProgramID = model.ProgramID,
                ProgramName = model.ProgramName,
                ProgramCode = model.ProgramCode,
                UniID = uniID,
                CreatedAt = DateTime.UtcNow
            };

            _db.Programs.Add(program);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Program created successfully!";
            return RedirectToAction(nameof(ManageProgram));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProgram(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var program = await _db.Programs
                .Where(p => p.ID == id && p.UniID == uniID)
                .FirstOrDefaultAsync();

            if (program == null)
                return NotFound();

            return View("~/Views/Dashboards/UA/UpdateProgram.cshtml", program);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgram(Programs model)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var program = await _db.Programs
                .Where(p => p.ID == model.ID && p.UniID == uniID)
                .FirstOrDefaultAsync();

            if (program == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View("~/Views/Dashboards/UA/UpdateProgram.cshtml", model);

            program.ProgramID = model.ProgramID;
            program.ProgramName = model.ProgramName;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Program updated successfully!";
            return RedirectToAction(nameof(ViewProgram), new { id = program.ID });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProgram(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var program = await _db.Programs
                .Where(p => p.ID == id && p.UniID == uniID)
                .FirstOrDefaultAsync();

            if (program == null)
                return NotFound();

            _db.Programs.Remove(program);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Program deleted successfully!";
            return RedirectToAction(nameof(ManageProgram));
        }

        // ============== MANAGE COURSES ==============
        [HttpGet]
        public async Task<IActionResult> ManageCourse(string? search = null)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var query = _db.Courses
                .Include(c => c.Programs)
                .Include(c => c.Module)
                .Where(c => c.Programs != null && c.Programs.UniID == uniID);

            // Add search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(c =>
                    (c.CourseID != null && c.CourseID.ToLower().Contains(searchLower)) ||
                    (c.CourseName != null && c.CourseName.ToLower().Contains(searchLower)));
            }

            var courses = await query
                .Select(c => new CourseMasterVm.CourseRow
                {
                    ID = c.ID,
                    CourseID = c.CourseID ?? "-",
                    CourseName = c.CourseName ?? "-",
                    ProgramName = c.Programs != null ? c.Programs.ProgramName ?? "-" : "-",
                    ModuleCount = c.Module != null ? 1 : 0
                })
                .ToListAsync();

            var vm = new CourseMasterVm { Courses = courses };
            ViewBag.SearchQuery = search ?? string.Empty;

            return View("~/Views/Dashboards/UA/ManageCourse.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> ViewCourse(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var course = await _db.Courses
                .Include(c => c.Programs)
                .Where(c => c.ID == id && c.Programs != null && c.Programs.UniID == uniID)
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound();

            var vm = new CourseDetailVm
            {
                ID = course.ID,
                CourseID = course.CourseID ?? string.Empty,
                CourseName = course.CourseName ?? string.Empty,
                ProgramID = course.ProgramID ?? string.Empty,
                ProgramName = course.Programs?.ProgramName ?? "-"
            };

            return View("~/Views/Dashboards/UA/ViewCourse.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Programs = await _db.Programs
                .Where(p => p.UniID == uniID)
                .ToListAsync();

            var vm = new CreateEditCourseVm();
            vm.Programs = await _db.Programs
                .Where(p => p.UniID == uniID)
                .Select(p => new SelectListItem { Value = p.ProgramID, Text = p.ProgramName })
                .ToListAsync();
            return View("~/Views/Dashboards/UA/CreateCourse.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CreateEditCourseVm model)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            ModelState.Remove("CourseID");
            ModelState.Remove("CourseCode");

            if (string.IsNullOrEmpty(model.CourseID))
            {
                model.CourseID = $"CRS_{DateTime.UtcNow.Ticks.ToString().Substring(0, 8)}";
            }

            if (!ModelState.IsValid)
            {
                model.Programs = await _db.Programs
                    .Where(p => p.UniID == uniID)
                    .Select(p => new SelectListItem { Value = p.ProgramID, Text = p.ProgramName })
                    .ToListAsync();
                return View("~/Views/Dashboards/UA/CreateCourse.cshtml", model);
            }

            var course = new Course
            {
                CourseID = model.CourseID,
                CourseName = model.CourseName,
                CourseCode = model.CourseCode,
                ProgramID = model.ProgramID
            };

            _db.Courses.Add(course);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Course created successfully!";
            return RedirectToAction(nameof(ManageCourse));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCourse(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var course = await _db.Courses
                .Include(c => c.Programs)
                .Where(c => c.ID == id && c.Programs != null && c.Programs.UniID == uniID)
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound();

            ViewBag.Programs = await _db.Programs
                .Where(p => p.UniID == uniID)
                .ToListAsync();

            return View("~/Views/Dashboards/UA/UpdateCourse.cshtml", course);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourse(Course model)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var course = await _db.Courses
                .Include(c => c.Programs)
                .Where(c => c.ID == model.ID && c.Programs != null && c.Programs.UniID == uniID)
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Programs = await _db.Programs
                    .Where(p => p.UniID == uniID)
                    .ToListAsync();
                return View("~/Views/Dashboards/UA/UpdateCourse.cshtml", model);
            }

            course.CourseID = model.CourseID;
            course.CourseName = model.CourseName;
            course.ProgramID = model.ProgramID;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Course updated successfully!";
            return RedirectToAction(nameof(ViewCourse), new { id = course.ID });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var course = await _db.Courses
                .Include(c => c.Programs)
                .Where(c => c.ID == id && c.Programs != null && c.Programs.UniID == uniID)
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound();

            _db.Courses.Remove(course);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Course deleted successfully!";
            return RedirectToAction(nameof(ManageCourse));
        }

        // ============== MANAGE EXPERTISE ==============
        [HttpGet]
        public async Task<IActionResult> ManageExpertise()
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var experts = await _db.StaffProfiles
                .Include(s => s.User)
                .Include(s => s.StaffModules)
                .Where(s => s.User != null && s.User.UniID == uniID)
                .Select(s => new ExpertiseMasterVm.ExpertiseRow
                {
                    ID = s.ID,
                    StaffName = s.User != null ? s.User.Name ?? "-" : "-",
                    Email = s.User != null ? s.User.Email ?? "-" : "-",
                    Modules = (s.StaffModules != null && s.StaffModules.Any())
                                ? string.Join(", ", s.StaffModules.Select(sm => sm.ModuleID ?? "-"))
                                : "-",
                    ModuleCount = s.StaffModules != null ? s.StaffModules.Count : 0
                })
                .ToListAsync();

            var vm = new ExpertiseMasterVm { Experts = experts };
            return View("~/Views/Dashboards/UA/ManageExpertise.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> ViewExpertise(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var staff = await _db.StaffProfiles
                .Include(s => s.User)
                .Include(s => s.StaffModules)
                    .ThenInclude(sm => sm.Module)
                    .ThenInclude(m => m != null ? m.Course : null)
                .Where(s => s.ID == id && s.User != null && s.User.UniID == uniID)
                .FirstOrDefaultAsync();

            if (staff == null)
                return NotFound();

            var vm = new ExpertiseDetailVm
            {
                StaffProfileID = staff.ID,
                UserID = staff.UserID,
                StaffName = staff.User?.Name ?? "-",
                Email = staff.User?.Email ?? "-",
                Modules = staff.StaffModules?.Select(sm => new ExpertiseDetailVm.ModuleItem
                {
                    ModuleID = sm.Module?.ID.ToString() ?? string.Empty,
                    ModuleName = sm.Module?.ModuleName ?? "-",
                    CourseName = sm.Module?.Course?.CourseName ?? "-"
                }).ToList() ?? new List<ExpertiseDetailVm.ModuleItem>()
            };

            return View("~/Views/Dashboards/UA/ViewExpertise.cshtml", vm);
        }

        // ============== MANAGE UNIVERSITY CONSTRAINTS ==============
        [HttpGet]
        public async Task<IActionResult> SetUniversityConstraints()
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var constraints = await _db.GlobalUniConstraints
                .Select(c => new UniversityConstraintsVm.ConstraintRow
                {
                    ID = c.ID,
                    PTeamSize = c.PTeamSize ?? "-",
                    SLoadCap = c.SLoadCap ?? "-",
                    ALoadCap = c.ALoadCap ?? "-",
                    PrefRankLimit = c.PrefRankLimit ?? "-"
                })
                .ToListAsync();

            var vm = new UniversityConstraintsVm { Constraints = constraints };
            return View("~/Views/Dashboards/UA/SetUniversityConstraints.cshtml", vm);
        }

        // ============== MANAGE REPORTS ==============
        [HttpGet]
        public Task<IActionResult> ManageReports()
        {
            ViewBag.Message = "Reports functionality coming soon!";
            return Task.FromResult<IActionResult>(View("~/Views/Dashboards/UA/ManageReports.cshtml"));
        }

        [HttpGet]
        public Task<IActionResult> DownloadReports()
        {
            TempData["Info"] = "Download reports functionality coming soon!";
            return Task.FromResult<IActionResult>(RedirectToAction(nameof(ManageReports)));
        }

        // ============== PLATFORM FEEDBACK ==============
        [HttpGet]
        public Task<IActionResult> ProvideFeedback()
        {
            return Task.FromResult<IActionResult>(View("~/Views/Dashboards/UA/ProvideFeedback.cshtml"));
        }
    }
}