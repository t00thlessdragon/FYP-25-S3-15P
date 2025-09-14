using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

public class AccountController : Controller
{
    private readonly SmartDbContext _db;
    public AccountController(SmartDbContext db) => _db = db;

    [HttpGet, AllowAnonymous]
    public IActionResult Login() => View(new Login());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(Login model)
    {
        if (!ModelState.IsValid) return View(model);

        var normalized = model.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailNormalized == normalized);
        if (user == null || user.IsLocked || !string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("", "Invalid login.");
            return View(model);
        }

        // TODO: replace with hash verification later
        if (!string.Equals(user.Password, model.Password))
        {
            ModelState.AddModelError("", "Invalid login.");
            return View(model);
        }

        var roleName = await _db.Roles
            .Where(r => r.RoleId == user.RoleId)
            .Select(r => r.Name)
            .FirstOrDefaultAsync() ?? "";

        // ✅ Build claims for cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.Name) ? user.Email : user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, roleName)
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,                 // or model.RememberMe if you add it
                ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
            });

        // Route by role
        if (string.Equals(roleName, "Platform Admin", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "PADashboard");

        return RedirectToAction("Index", "Home");
    }

    // ✅ Logout
    // GET: /Account/Logout (optional confirm page you already have)
    [Authorize]
    [HttpGet, ActionName("Logout")]
    public IActionResult LogoutGet() => View("Logout");

// POST: /Account/Logout (sign out, then show success popup)
    [Authorize]
    [HttpPost, ValidateAntiForgeryToken, ActionName("Logout")]
    public async Task<IActionResult> LogoutPost()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // Instead of redirecting immediately, show a “LoggedOut” page with a modal
        return View("Logout");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
