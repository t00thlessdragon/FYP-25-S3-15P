using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace FYP_25_S3_15P.Data
{
    public class DatabaseSeeder
    {
        private readonly SmartDbContext _db;

        public DatabaseSeeder(SmartDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync()
        {
            // Seed Roles first
            await SeedRoles();

            // Then seed Users
            await SeedUsers();
        }

        private async Task SeedRoles()
        {
            var roles = new List<string>
            {
                "Platform Admin",
                "University Admin",
                "Student",
                "Assessor"
            };

            foreach (var roleName in roles)
            {
                // Check if role already exists
                var exists = await _db.Roles.AnyAsync(r => r.Name == roleName);

                if (!exists)
                {
                    var role = new Role
                    {
                        Name = roleName,
                        Description = $"Test {roleName} role"
                    };

                    _db.Roles.Add(role);
                }
            }

            await _db.SaveChangesAsync();
        }

        private async Task SeedUsers()
        {
            // Get role IDs
            var platformAdminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Platform Admin");
            var universityAdminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "University Admin");
            var studentRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
            var assessorRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Assessor");

            // Create test users
            await CreateUserIfNotExists(
                email: "platformadmin@test.com",
                password: "Admin123!",  // TODO: Hash this properly
                name: "Platform Admin Test",
                roleId: platformAdminRole?.ID ?? 0
            );

            await CreateUserIfNotExists(
                email: "universityadmin@test.com",
                password: "Admin123!",
                name: "University Admin Test",
                roleId: universityAdminRole?.ID ?? 0
            );

            await CreateUserIfNotExists(
                email: "student@test.com",
                password: "Student123!",
                name: "Test Student",
                roleId: studentRole?.ID ?? 0
            );

            await CreateUserIfNotExists(
                email: "assessor@test.com",
                password: "Assessor123!",
                name: "Test Assessor",
                roleId: assessorRole?.ID ?? 0
            );

            await _db.SaveChangesAsync();
        }

        private async Task CreateUserIfNotExists(string email, string password, string name, int roleId)
        {
            var normalized = email.Trim().ToLowerInvariant();

            // Check if user exists by checking the normalized email
            var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalized);

            if (!exists)
            {
                var user = new User
                {
                    Email = email,
                    // ❌ DO NOT SET EmailNormalized - it's computed by the database
                    Password = password,  // ⚠️ TODO: Use proper password hashing!
                    Name = name,
                    RoleID = roleId,
                    Status = "Active",
                    IsLocked = false,
                    MustChangePassword = false,  // Set to false for test users
                    CreatedAt = DateTime.UtcNow
                };

                _db.Users.Add(user);
            }
        }
    }
}
