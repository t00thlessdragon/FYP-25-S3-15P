using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using FYP_25_S3_15P.Services;
using FYP_25_S3_15P.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // IPasswordHasher<User>
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Globalization;

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
                UniversityName = await _db.University
                    .Where(u => u.UniID == uniID)
                    .Select(u => u.UniName)
                    .FirstOrDefaultAsync() ?? "University",
                TotalUsers = await _db.Users.CountAsync(u => u.UniID == uniID),
                TotalPrograms = await _db.Programs.CountAsync(p => p.UniID == uniID),
                TotalCourses = await _db.Courses
                    .Include(c => c.Programs)
                    .Where(c => c.Programs.UniID == uniID)
                    .CountAsync(),
                TotalExperts = await _db.StaffProfiles
                    .Include(s => s.User)
                    .Where(s => s.User.UniID == uniID)
                    .CountAsync()
            };

            return View(vm);
        }

        // ============== MANAGE USERS ==============
        [HttpGet]
        public async Task<IActionResult> ManageUsersList(string search = "")
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
                    u.Name.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower));
            }

            var users = await query
                .Select(u => new UserMasterVm.Row
                {
                    ID = u.ID,
                    Name = u.Name,
                    Email = u.Email,
                    UniName = u.University != null ? u.University.UniName : "-",
                    RoleID = u.RoleID,
                    Role = u.Role != null ? u.Role.Name : "-",
                    Status = u.Status ?? "-",
                    IsLocked = u.IsLocked,
                    LastLogin = u.LastLogin
                })
                .ToListAsync();

            var vm = new UserMasterVm { Users = users };
            ViewBag.SearchQuery = search;

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ViewUserProfile(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var user = await _db.Users
                .Include(u => u.Role)
                .Include(u => u.University)
                .Include(u => u.CreatedBy)
                .Where(u => u.ID == id && u.UniID == uniID)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            var vm = new UserDetailVm
            {
                ID = user.ID,
                Name = user.Name,
                Email = user.Email,
                UniversityName = user.University?.UniName ?? "-",
                Role = user.Role?.Name ?? "-",
                RoleID = user.RoleID,
                Status = user.Status ?? "-",
                IsLocked = user.IsLocked,
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy.HasValue
                    ? (_db.Users
                    .Where(u => u.ID == user.CreatedBy.Value)
                    .Select(u => u.Name)
                    .FirstOrDefault() ?? "System"): "System"
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUserAccount()
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.Roles = await _db.Roles.ToListAsync();
            return View(new User());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserAccount(User model)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _db.Roles.ToListAsync();
                return View(model);
            }

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            model.UniID = uniID;
            model.CreatedBy = currentUserId;
            model.CreatedAt = DateTime.UtcNow;
            // TODO: Hash password properly

            _db.Users.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "User created successfully!";
            return RedirectToAction(nameof(ManageUsersList));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateUserAccount(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var user = await _db.Users
                .Where(u => u.ID == id && u.UniID == uniID)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            ViewBag.Roles = await _db.Roles.ToListAsync();
            return View(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserAccount(User model)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var user = await _db.Users
                .Where(u => u.ID == model.ID && u.UniID == uniID)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _db.Roles.ToListAsync();
                return View(model);
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.RoleID = model.RoleID;
            user.Status = model.Status;
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

        // ============== MANAGE PROGRAMS ==============
        [HttpGet]
        public async Task<IActionResult> ManageProgram()
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var programs = await _db.Programs
                .Include(p => p.Course)
                .Where(p => p.UniID == uniID)
                .Select(p => new ProgramMasterVm.ProgramRow
                {
                    ID = p.ID,
                    ProgramID = p.ProgramID ?? "-",
                    ProgramName = p.ProgramName ?? "-",
                    CourseCount = _db.Courses.Count(c => c.ProgramID == p.ProgramID)
                })
                .ToListAsync();

            var vm = new ProgramMasterVm { Programs = programs };
            return View(vm);
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

            return View(vm);
        }

        [HttpGet]
        public IActionResult CreateProgram()
        {
            return View(new Program());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProgram(Programs model)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
                return View(model);

            model.UniID = uniID;
            _db.Programs.Add(model);
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

            return View(program);
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
                return View(model);

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
        public async Task<IActionResult> ManageCourse()
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var courses = await _db.Courses
                .Include(c => c.Programs)
                .Include(c => c.Module)
                .Where(c => c.Programs.UniID == uniID)
                .Select(c => new CourseMasterVm.CourseRow
                {
                    ID = c.ID,
                    CourseID = c.CourseID ?? "-",
                    CourseName = c.CourseName ?? "-",
                    ProgramName = c.Programs != null ? c.Programs.ProgramName : "-",
                    ModuleCount = c.Module != null ? 1 : 0
                })
                .ToListAsync();

            var vm = new CourseMasterVm { Courses = courses };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ViewCourse(int id)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var course = await _db.Courses
                .Include(c => c.Programs)
                .Where(c => c.ID == id && c.Programs.UniID == uniID)
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

            return View(vm);
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

            return View(new Course());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course model)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Programs = await _db.Programs
                    .Where(p => p.UniID == uniID)
                    .ToListAsync();
                return View(model);
            }

            _db.Courses.Add(model);
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
                .Where(c => c.ID == id && c.Programs.UniID == uniID)
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound();

            ViewBag.Programs = await _db.Programs
                .Where(p => p.UniID == uniID)
                .ToListAsync();

            return View(course);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourse(Course model)
        {
            var uniID = await GetCurrentUserUniIDAsync();
            if (string.IsNullOrEmpty(uniID))
                return RedirectToAction("AccessDenied", "Account");

            var course = await _db.Courses
                .Include(c => c.Programs)
                .Where(c => c.ID == model.ID && c.Programs.UniID == uniID)
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Programs = await _db.Programs
                    .Where(p => p.UniID == uniID)
                    .ToListAsync();
                return View(model);
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
                .Where(c => c.ID == id && c.Programs.UniID == uniID)
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
                .Where(s => s.User.UniID == uniID)
                .Select(s => new ExpertiseMasterVm.ExpertiseRow
                {
                    ID = s.ID,
                    StaffName = s.User != null ? s.User.Name : "-",
                    Email = s.User != null ? s.User.Email : "-",
                    Modules = (s.StaffModules != null && s.StaffModules.Any())
                                ? string.Join(", ", s.StaffModules.Select(sm => sm.ModuleID ?? "-"))
                                : "-",
                    ModuleCount = s.StaffModules.Count
                })
                .ToListAsync();

            var vm = new ExpertiseMasterVm { Experts = experts };
            return View(vm);
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
                    .ThenInclude(m => m.Course)
                .Where(s => s.ID == id && s.User.UniID == uniID)
                .FirstOrDefaultAsync();

            if (staff == null)
                return NotFound();

            var vm = new ExpertiseDetailVm
            {
                StaffProfileID = staff.ID,
                UserID = staff.UserID = 0,
                StaffName = staff.User?.Name ?? "-",
                Email = staff.User?.Email ?? "-",
                Modules = staff.StaffModules.Select(sm => new ExpertiseDetailVm.ModuleItem
                {
                    ModuleID = sm.Module?.ID.ToString() ?? string.Empty,
                    ModuleName = sm.Module?.ModuleName ?? "-",
                    CourseName = sm.Module?.Course?.CourseName ?? "-"
                }).ToList()
            };

            return View(vm);
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
            return View(vm);
        }
    }
}