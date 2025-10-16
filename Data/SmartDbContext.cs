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
        public DbSet<University> University { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<FAQ> FAQs { get; set; }= default!;
        public DbSet<UserRole> UserRoles { get; set; } = default!;

        public DbSet<FYPTemplates> FYPTemplates { get; set; } = default!;

        public DbSet<Programs> Programs { get; set; } = default!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Make dbo the default schema so we don't have to repeat it
            modelBuilder.HasDefaultSchema("dbo");

            // existing mappings …
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
                e.ToTable("ApplicationForm");  // default schema = dbo
                e.HasOne(a => a.Plan).WithMany().HasForeignKey(a => a.PlanID);
                e.HasOne(a => a.University).WithMany(u => u.ApplicationForms).HasForeignKey(a => a.UniID);
            });

            modelBuilder.Entity<University>(e =>
            {
                e.ToTable("University");
            });

            // USERS → dbo.Users   (removed "Smart" schema)
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
                e.HasKey(r => r.Id);
                e.Property(r => r.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<UserRole>(e =>
            {
                e.ToTable("UserRole");

                e.HasKey(ur => ur.ID);

                // Each UserRole has one User and one Role
                e.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID)
                .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleID)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FYPTemplates>(e =>
            {
                e.ToTable("FYPTemplates");

                e.HasKey(f => f.ID);

                e.Property(f => f.ProjectName)
                    .HasMaxLength(255)
                    .IsRequired();

                e.Property(f => f.Description)
                    .HasColumnType("nvarchar(max)");

                e.Property(f => f.CreatedBy)
                    .HasMaxLength(100);

                e.Property(f => f.CreatedAt)
                    .IsRequired();

                e.Property(f => f.UpdatedAt)
                    .IsRequired(false);

                // Optional relationships
                e.HasOne(f => f.University)
                    .WithMany()
                    .HasForeignKey(f => f.UniID)
                    .HasPrincipalKey(u => u.ID)     // explicitly link to University.ID
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(f => f.Programs)
                    .WithMany()
                    .HasForeignKey(f => f.ProgID)
                    .HasPrincipalKey(p => p.ID)     // explicitly link to Programs.ID
                    .OnDelete(DeleteBehavior.Restrict);
            });
            
            modelBuilder.Entity<Programs>(e =>
            {
                e.ToTable("Programs");

                e.HasKey(p => p.ID);

                e.Property(p => p.ProgramName)
                    .HasMaxLength(255)
                    .IsRequired();

                e.Property(p => p.ProgramCode)
                    .HasMaxLength(50)
                    .IsRequired();

                e.Property(p => p.CreatedBy)
                    .HasMaxLength(100);

                e.Property(p => p.UpdatedBy)
                    .HasMaxLength(100);

                e.Property(p => p.CreatedAt)
                    .IsRequired();

                e.HasOne(p => p.University)
                    .WithMany()
                    .HasForeignKey(p => p.UniID);
            });
        }
    }
}
