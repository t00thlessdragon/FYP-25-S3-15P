using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;

public class AccountController : Controller
{
    private readonly SmartDbContext _db;
    public AccountController(SmartDbContext db) => _db = db;

    [HttpGet, AllowAnonymous]
    public IActionResult Login()
    {
        // Already signed in? Send to home (or dashboard).
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new Login());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(Login model)
    {
        if (!ModelState.IsValid) return View(model);

        var normalized = (model.Email ?? "").Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailNormalized == normalized);
        var isActive = string.Equals(user?.Status, "Active", StringComparison.OrdinalIgnoreCase);

        if (user == null || user.IsLocked || !isActive)
        {
            ModelState.AddModelError("", "Invalid login.");
            return View(model);
        }

        // TODO: replace with a proper password hash check (IPasswordHasher<User>)
        if (!string.Equals(user.Password, model.Password))
        {
            ModelState.AddModelError("", "Invalid login.");
            return View(model);
        }

        var roleName = await _db.Roles
            .Where(r => r.RoleId == user.RoleId)
            .Select(r => r.Name)
            .FirstOrDefaultAsync() ?? string.Empty;

        // Build claims for cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.Name) ? user.Email : user.Name),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, roleName)
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Sign in on the same (default) cookie scheme
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
            });

        // ✅ Record last login (UTC) so dashboards can show it
        user.LastLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Route by role if needed
        if (string.Equals(roleName, "Platform Admin", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "PADashboard");

        return RedirectToAction("Index", "Home");
    }

    // Optional GET confirmation page
    [Authorize]
    [HttpGet, ActionName("Logout")]
    public IActionResult LogoutGet() => View("Logout");

    // POST: actually sign out
    [Authorize]
    [HttpPost, ValidateAntiForgeryToken, ActionName("Logout")]
    public async Task<IActionResult> LogoutPost()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return View("Logout");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
