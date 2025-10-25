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
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<FYP_25_S3_15P.Models.UniversityConstraint> UniversityConstraints { get; set; }
        public DbSet<FYP_25_S3_15P.Models.Program> Programs { get; set; } = null!;
        public DbSet<FYP_25_S3_15P.Models.Course>  Courses  { get; set; } = null!;
        public DbSet<FYP_25_S3_15P.Models.Module>  Modules  { get; set; } = null!;
        public DbSet<ProjectTemplate> ProjectTemplates => Set<ProjectTemplate>();
        public DbSet<ProjectTemplateModule> ProjectTemplateModules => Set<ProjectTemplateModule>();
        public DbSet<CourseConstraint> CourseConstraints { get; set; } = null!;
        public DbSet<ProgramConstraint> ProgramConstraints { get; set; } = null!;
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectModule> ProjectModules => Set<ProjectModule>();
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<StaffProfile>      StaffProfiles     { get; set; } = default!;
        public DbSet<StudentProfile>    StudentProfiles   { get; set; } = default!;
        public DbSet<Session>           Sessions          { get; set; } = default!;
        public DbSet<StaffModules>   StaffModules   { get; set; } = default!;
        public DbSet<StudentModules> StudentModules { get; set; } = default!;
        public DbSet<UniSession> UniSession { get; set; }

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
                e.ToTable("ApplicationForm");
                e.HasOne(a => a.Plan).WithMany().HasForeignKey(a => a.PlanID);
                e.HasOne(a => a.University).WithMany(u => u.ApplicationForms).HasForeignKey(a => a.UniID);
            });

            modelBuilder.Entity<University>(e =>
            {
                e.ToTable("Universities");
            });

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

            // ---------- Program ----------
            // ---------- Program ----------
modelBuilder.Entity<FYP_25_S3_15P.Models.Program>(e =>
{
    e.ToTable("Program");
    e.HasKey(x => x.ID);

    e.Property(x => x.ProgramCode).HasMaxLength(50).IsRequired();
    e.Property(x => x.ProgramName).HasMaxLength(200).IsRequired();

    // unique per university (indexes still fine)
    e.HasIndex(x => new { x.UniID, x.ProgramCode }).IsUnique();
    e.HasIndex(x => new { x.UniID, x.ProgramName }).IsUnique();

    // Needed because you use HasPrincipalKey(p => p.ProgramCode) elsewhere
    e.HasAlternateKey(x => x.ProgramCode);
});


            // ---------- Course ----------
           // ---------- Course ----------
modelBuilder.Entity<FYP_25_S3_15P.Models.Course>(e =>
{
    e.ToTable("Course");
    e.HasKey(x => x.ID);

    e.Property(x => x.CourseCode).HasMaxLength(50).IsRequired();
    e.Property(x => x.CourseName).HasMaxLength(200).IsRequired();

    e.HasOne(x => x.Program)
        .WithMany(p => p.Courses)
        .HasForeignKey(x => x.ProgramID)
        .HasPrincipalKey(p => p.ID)
        .OnDelete(DeleteBehavior.Cascade);

    // CourseCode unique within a Program (no CourseID constraint anymore)
    e.HasIndex(x => new { x.ProgramID, x.CourseCode }).IsUnique();

    // Many-to-many Course <-> Module via CourseModule
    e.HasMany(c => c.Modules)
        .WithMany(m => m.Courses)
        .UsingEntity<Dictionary<string, object>>(
            "CourseModule",
            right => right
                .HasOne<FYP_25_S3_15P.Models.Module>()
                .WithMany()
                .HasForeignKey("ModuleID")
                .HasPrincipalKey(m => m.ID)
                .OnDelete(DeleteBehavior.Cascade),
            left => left
                .HasOne<FYP_25_S3_15P.Models.Course>()
                .WithMany()
                .HasForeignKey("CourseID")
                .HasPrincipalKey(c => c.ID)
                .OnDelete(DeleteBehavior.Cascade),
            join =>
            {
                join.ToTable("CourseModule");
                join.HasKey("CourseID", "ModuleID");
            });
});


            // ---------- Module ----------
          modelBuilder.Entity<FYP_25_S3_15P.Models.Module>(e =>
{
    e.ToTable("Module");
    e.HasKey(x => x.ID);
    e.Property(x => x.ModuleCode).HasMaxLength(50).IsRequired();
    e.Property(x => x.ModuleName).HasMaxLength(200).IsRequired();

    // REMOVE this line (no more ModuleID column)
    // e.HasIndex(x => x.ModuleID).IsUnique();

    // Keep these (global uniqueness by business keys)
    e.HasIndex(x => x.ModuleCode).IsUnique();
    e.HasIndex(x => x.ModuleName).IsUnique();
});



            modelBuilder.Entity<ProjectTemplate>(e =>
                    {
                        e.ToTable("project_template");
                        e.HasKey(x => x.Id);

                        e.HasIndex(x => x.TemplateCode).IsUnique();
                        e.Property(x => x.TemplateCode).HasMaxLength(20).IsRequired();
                        e.Property(x => x.ProgramCode).HasMaxLength(20).IsRequired();
                        e.Property(x => x.Status).HasMaxLength(20).IsRequired();

                        // soft reference by ProgramCode (no cascade)
                        e.HasOne<FYP_25_S3_15P.Models.Program>()
                        .WithMany()
                        .HasPrincipalKey(p => p.ProgramCode)
                        .HasForeignKey(x => x.ProgramCode)
                        .OnDelete(DeleteBehavior.NoAction);
                    });

            modelBuilder.Entity<ProjectTemplateModule>(e =>
            {
                e.ToTable("project_template_module");
                e.HasKey(x => new { x.ProjectTemplateId, x.ModuleCode });

                e.HasOne(x => x.ProjectTemplate)
                    .WithMany(t => t.TemplateModules)
                    .HasForeignKey(x => x.ProjectTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                // soft reference by ModuleCode (no cascade)
                e.HasOne(x => x.Module)
                    .WithMany()
                    .HasPrincipalKey(m => m.ModuleCode)
                    .HasForeignKey(x => x.ModuleCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            modelBuilder.Entity<CourseConstraint>()
                .ToTable("CourseConstraints")
                .HasOne(cc => cc.Course)
                .WithMany()
                .HasForeignKey(cc => cc.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Join table
            modelBuilder.Entity<ProjectModule>().ToTable("ProjectModules");
            modelBuilder.Entity<ProjectModule>()
                .HasKey(pm => new { pm.ProjectId, pm.ModuleCode });

            modelBuilder.Entity<ProjectModule>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.ProjectModules)   // <-- Project *does* have this collection
                .HasForeignKey(pm => pm.ProjectId)
                .HasPrincipalKey(p => p.ProjectId);

            modelBuilder.Entity<ProjectModule>()
                .HasOne(pm => pm.Module)
                .WithMany()                        // <-- no back-collection on Module
                .HasForeignKey(pm => pm.ModuleCode)
                .HasPrincipalKey(m => m.ModuleCode);  // FK -> alternate key (ModuleCode)


            modelBuilder.Entity<UniSession>()
                .HasKey(us => new { us.UniID, us.Year, us.SessionID });

        }
    }
}
