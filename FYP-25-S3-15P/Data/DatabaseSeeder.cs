using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

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
            // 1. Seed University first
            await SeedUniversity();

            // 2. Seed Roles
            await SeedRoles();

            // 3. Then seed Users
            await SeedUsers();
        }

        private async Task SeedUniversity()
        {
            // Check if test university exists
            var exists = await _db.Universities.AnyAsync(u => u.UniID == "TEST001");

            if (!exists)
            {
                var university = new University
                {
                    UniID = "TEST001",
                    UniName = "Test University",
                    UnivCode = "TEST"
                };
                _db.Universities.Add(university);
                await _db.SaveChangesAsync();
                Console.WriteLine("✅ Test University created");
            }
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
                var exists = await _db.Roles.AnyAsync(r => r.Name == roleName);
                if (!exists)
                {
                    var role = new Role
                    {
                        Name = roleName,
                        Description = $"{roleName} role"
                    };
                    _db.Roles.Add(role);
                    Console.WriteLine($"✅ Role '{roleName}' created");
                }
            }
            await _db.SaveChangesAsync();
        }

        private async Task SeedUsers()
        {
            // Get the test university
            var testUniID = "TEST001";

            // Get role IDs
            var platformAdminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Platform Admin");
            var universityAdminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "University Admin");
            var studentRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
            var assessorRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Assessor");

            // Create test users
            await CreateUserIfNotExists(
                email: "platformadmin@test.com",
                password: "Admin123!",
                name: "Platform Admin Test",
                roleId: platformAdminRole?.ID ?? 0,
                uniID: testUniID
            );

            await CreateUserIfNotExists(
                email: "universityadmin@test.com",
                password: "Admin123!",
                name: "University Admin Test",
                roleId: universityAdminRole?.ID ?? 0,
                uniID: testUniID
            );

            await CreateUserIfNotExists(
                email: "student@test.com",
                password: "Student123!",
                name: "Test Student",
                roleId: studentRole?.ID ?? 0,
                uniID: testUniID
            );

            await CreateUserIfNotExists(
                email: "assessor@test.com",
                password: "Assessor123!",
                name: "Test Assessor",
                roleId: assessorRole?.ID ?? 0,
                uniID: testUniID
            );

            await _db.SaveChangesAsync();
        }

        private async Task CreateUserIfNotExists(string email, string password, string name, int roleId, string uniID)
        {
            var normalized = email.Trim().ToLowerInvariant(); // Email normalization is uppercase

            // Check if user exists
            var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalized);

            if (!exists)
            {
                var user = new User
                {
                    UniID = uniID,
                    Email = email,
                    Password = password,  // ⚠️ TODO: Hash this properly!
                    Name = name,
                    RoleID = roleId,
                    Status = "Active",
                    IsLocked = false,
                    MustChangePassword = false,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                Console.WriteLine($"✅ User '{email}' created with EmailNormalized: {user.EmailNormalized}");
            }
        }
    }
}