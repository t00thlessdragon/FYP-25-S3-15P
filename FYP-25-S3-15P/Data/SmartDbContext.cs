using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Data
{
    public class SmartDbContext : DbContext
    {
        public SmartDbContext(DbContextOptions<SmartDbContext> options) : base(options) { }

        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = default!;
        public DbSet<Feature> Features { get; set; } = default!;
        public DbSet<PlanFeature> PlanFeatures { get; set; } = default!;
        public DbSet<ApplicationForm> ApplicationForms { get; set; } = default!;
        public DbSet<University> Universities { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<FAQ> FAQs { get; set; } = default!;
        public DbSet<UniversityProgram> UniversityPrograms { get; set; } = default!;
        public DbSet<Course> Courses { get; set; } = default!;
        public DbSet<StaffProfile> StaffProfiles { get; set; } = default!;
        public DbSet<StudentProfile> StudentProfiles { get; set; } = default!;
        public DbSet<StaffModules> StaffModules { get; set; } = default!;
        public DbSet<Module> Modules { get; set; } = default!;
        public DbSet<Session> Sessions { get; set; }
        public DbSet<UniSession> UniSession { get; set; }
        public DbSet<StudentModules> StudentModules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<SubscriptionPlan>()
                .ToTable("subscriptionPlans").HasKey(p => p.PlanID);

            modelBuilder.Entity<Feature>()
                .ToTable("Features").HasKey(f => f.FeatureID);

            modelBuilder.Entity<PlanFeature>()
                .ToTable("PlanFeatures").HasKey(pf => new { pf.PlanID, pf.FeatureID });

            modelBuilder.Entity<PlanFeature>()
                .HasOne(pf => pf.Plan).WithMany(p => p.PlanFeatures)
                .HasForeignKey(pf => pf.PlanID).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlanFeature>()
                .HasOne(pf => pf.Feature).WithMany(f => f.PlanFeatures)
                .HasForeignKey(pf => pf.FeatureID).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationForm>(e =>
            {
                e.ToTable("ApplicationForm");
                e.HasOne(a => a.Plan).WithMany().HasForeignKey(a => a.PlanID);
                e.HasOne(a => a.University).WithMany(u => u.ApplicationForms).HasForeignKey(a => a.UniID);
            });

            modelBuilder.Entity<University>(e =>
            {
                e.ToTable("Universities");
            });

            modelBuilder.Entity<UniSession>()
        .HasKey(us => new { us.UniID, us.Year, us.SessionID });

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(u => u.Id);

                e.Property(u => u.Email).HasMaxLength(256);
                e.Property(u => u.EmailNormalized).HasMaxLength(256);
                e.HasIndex(u => u.EmailNormalized);

                e.Property(u => u.Password).HasMaxLength(256);
                e.Property(u => u.Status).HasMaxLength(50);
                e.Property(u => u.EmailDomain).HasMaxLength(256);

                e.Property(u => u.RowVersion).IsRowVersion();
            });

            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("Roles");
                e.HasKey(r => r.RoleId);
                e.Property(r => r.Name).HasMaxLength(100);
            });
        }
    }
}
