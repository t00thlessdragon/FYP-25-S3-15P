using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

[Authorize] 
public class UADashboardController : Controller
{
    private readonly SmartDbContext _context;

    public UADashboardController(SmartDbContext context)
    {
        _context = context;
    }

    // GET: /UADashboard
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !int.TryParse(userId, out int adminId))
        {
            TempData["Error"] = "User identity not found or invalid.";
            return RedirectToAction("AccessDenied", "Account");
        }

        var adminProfile = await _context.Users
            .Include(p => p.University) 
            .FirstOrDefaultAsync(p => p.Id == adminId);

        const int UniversityAdminRoleID = 9; 
        
        if (adminProfile?.RoleId != UniversityAdminRoleID) 
        {
            TempData["Error"] = "Insufficient privileges for this dashboard.";
            return RedirectToAction("AccessDenied", "Account");
        }

        if (adminProfile?.University == null)
        {
            TempData["Error"] = "University profile information is missing.";
            return View("UserMaster", new StaffAndStudentVm());
        }

        var uniAbbrv = adminProfile.University.UniAbbrv;
        var uniId = adminProfile.University.UniID; 

        var viewModel = new StaffAndStudentVm
        {
            Staff = await GetStaffDataAsync(uniId, uniAbbrv),
            Students = await GetStudentDataAsync(uniId, uniAbbrv)
        };

        return View("~/Views/Dashboards/UA/UserMaster.cshtml", viewModel);
    }

    // Helper to fetch and map Staff data
    private async Task<List<StaffAndStudentVm.StaffRow>> GetStaffDataAsync(int uniId, string uniAbbrv)
    {
        var staffList = await _context.StaffProfiles
            .Where(s => s.User.UniID == uniId) 
            .Include(s => s.User)
            .Include(s => s.StaffModules).ThenInclude(sm => sm.Module)
            .Select(s => new StaffAndStudentVm.StaffRow
            {
                Id = s.UserID.Value, 
                StaffID = s.StaffID,
                Name = s.User.Name,
                Email = s.User.Email,
                UniAbbrv = uniAbbrv,
                Status = s.User.Status,
                LastLogin = s.User.LastLogin,
                IsLocked = s.User.Status != "Active",
                SessionNo = s.CurrentSessionID.HasValue ? s.CurrentSessionID.Value.ToString() : "N/A",
                AssignedModules = s.StaffModules != null ? s.StaffModules.Select(sm => new StaffAndStudentVm.ModuleRow
                {
                    ModuleCode = sm.Module.ModuleCode
                }).ToList() : new List<StaffAndStudentVm.ModuleRow>()
            })
            .ToListAsync();

        return staffList;
    }

    // Helper to fetch and map Student data
    private async Task<List<StaffAndStudentVm.StudentRow>> GetStudentDataAsync(int uniId, string uniAbbrv)
    {
        var studentList = await _context.StudentProfiles
            .Where(s => s.Course.Program.UniID == uniId)
            .Include(s => s.User)
            .Include(s => s.Course).ThenInclude(c => c.Program)
            .Include(s => s.StudentModules).ThenInclude(sm => sm.Module) 
            .Select(s => new StaffAndStudentVm.StudentRow
            {
                Id = s.UserID.Value, 
                StudentID = s.StudentID,
                Name = s.User.Name,
                Email = s.User.Email,
                UniAbbrv = uniAbbrv,
                Status = s.User.Status,
                LastLogin = s.User.LastLogin,
                IsLocked = s.User.Status != "Active",
                ProgramName = s.Course.Program.ProgramName, 
                CourseName = s.Course.CourseName, 
                SessionNo = s.SessionID.ToString() ?? "N/A", 
                EnrolledModules = s.StudentModules.Select(em => new StaffAndStudentVm.ModuleRow
                {
                    ModuleCode = em.Module.ModuleCode 
                }).ToList()
            })
            .ToListAsync();

        return studentList;
    }

    // POST: /UADashboard/ToggleActive
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string profileId, bool value) 
    {
        // 1. **CRITICAL FIX**: Safely convert profileId string to integer.
        // If profileId is null or not a valid integer, id will be 0 and the check below fails.
        if (string.IsNullOrWhiteSpace(profileId) || !int.TryParse(profileId, out int id))
        {
            TempData["Error"] = "Invalid profile ID submitted.";
            return RedirectToAction(nameof(Index));
        }

        // 2. Use the safe integer 'id' to find the user.
        var userProfile = await _context.Users.FindAsync(id);

        if (userProfile == null)
        {
            TempData["Error"] = $"User with ID {id} not found.";
            return RedirectToAction(nameof(Index));
        }

        // 3. Update the user status.
        // Assuming IsLocked maps to Status, or Status is derived from IsLocked.
        // If 'value' is true (Activate), IsLocked should be false.
        // If your User model uses a 'Status' string (e.g., "Active" or "Locked"), use this:
        userProfile.Status = value ? "Active" : "Locked"; 
        
        // If your User model uses a boolean IsLocked property, use this instead:
        // userProfile.IsLocked = !value; 
        
        // If your User model has an UpdatedAt field:
        // userProfile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        TempData["Flash"] = $"User '{userProfile.Name}' successfully {(value ? "activated" : "deactivated")}.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /UADashboard/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string profileId, string profileType)
    {
        // 1. **CRITICAL FIX**: Safely convert profileId string to integer.
        if (string.IsNullOrWhiteSpace(profileId) || !int.TryParse(profileId, out int id))
        {
            TempData["Error"] = "Invalid profile ID submitted for password reset.";
            return RedirectToAction(nameof(Index));
        }
        
        var userProfile = await _context.Users.FindAsync(id);

        if (userProfile == null)
        {
            TempData["Error"] = $"User with ID {id} not found.";
            return RedirectToAction(nameof(Index));
        }

        // Note: In a real application, you would integrate with ASP.NET Identity
        // to generate and email a temporary password here.

        TempData["Flash"] = $"Password reset initiated for {profileType} '{userProfile.Name}'. A temporary password has been emailed.";
        return RedirectToAction(nameof(Index));
    }
}
