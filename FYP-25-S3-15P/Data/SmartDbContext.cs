using FYP_25_S3_15P.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FYP_25_S3_15P.Data
{
    public class SmartDbContext : DbContext
    {
        public SmartDbContext(DbContextOptions<SmartDbContext> options) : base(options) { }

        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = default!;
        public DbSet<Feature> Features { get; set; } = default!;
        public DbSet<PlanFeature> PlanFeatures { get; set; } = default!;
        public DbSet<ApplicationForm> ApplicationForms { get; set; } = default!;
        public DbSet<FAQ> FAQs { get; set; } = default!;
        public DbSet<University> Universities { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<GlobalUniConstraint> GlobalUniConstraints { get; set; } = default!;
        public DbSet<Session> Sessions { get; set; } = default!;
        public DbSet<Course> Courses { get; set; } = default!;
        public DbSet<Programs> Programs { get; set; } = default!;
        public DbSet<Module> Modules { get; set; } = default!;
        public DbSet<FYPTopic> FYPTopics { get; set; } = default!;
        public DbSet<Preference> Preferences { get; set; } = default!;
        public DbSet<Group> Groups { get; set; } = default!;
        public DbSet<StaffModule> StaffModules { get; set; } = default!;
        public DbSet<StaffProfile> StaffProfiles { get; set; } = default!;
        public DbSet<StudentProfile> StudentProfiles { get; set; } = default!;
        public DbSet<UserRole> UserRoles { get; set; } = default!;
        public DbSet<FYPTemplates> FYPTemplates { get; set; } = default!;
        public DbSet<Assessment> Assessments { get; set; } = default!;
        public DbSet<Tasks> Tasks { get; set; } = default!;
        public DbSet<TaskEvaluation> TaskEvaluations { get; set; } = default!;
        public DbSet<UserGroups> UserGroups { get; set; } = default!;
        public DbSet<UniversityProgram> UniversityPrograms { get; set; } = default!;
        public DbSet<UniSession> UniSession { get; set; }
        public DbSet<StudentModules> StudentModules { get; set; }

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
                e.HasOne(a => a.University).WithMany(u => u.ApplicationForms).HasForeignKey(u => u.UniId).HasPrincipalKey(a => a.ID).OnDelete(DeleteBehavior.Restrict); ;
            });

            modelBuilder.Entity<University>(e =>
            {
                e.ToTable("Universities");
                e.HasKey(u => u.ID);
                e.HasAlternateKey(u => u.UniID); // set UniID as an alternate key

                e.Property(u => u.UniName).HasMaxLength(256);
                e.Property(u => u.UnivCode).HasMaxLength(50);
            });

            // USERS → dbo.Users   (removed "Smart" schema)
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(u => u.ID);

                e.Property(u => u.Email).HasMaxLength(256);
                e.Property(u => u.EmailNormalized).HasMaxLength(256);
                e.HasIndex(u => u.EmailNormalized);

                e.Property(u => u.Password).HasMaxLength(256);
                e.Property(u => u.Status).HasMaxLength(50);
                e.Property(u => u.EmailDomain).HasMaxLength(256);

                e.Property(u => u.RowVersion).IsRowVersion();

                e.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleID)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(u => u.University)
                    .WithMany()
                    .HasForeignKey(u => u.UniID)
                    .HasPrincipalKey(un => un.UniID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("Roles");
                e.HasKey(r => r.ID);
                e.Property(r => r.Name).HasMaxLength(100);
                e.HasIndex(r => r.Name).IsUnique();
            });

            modelBuilder.Entity<GlobalUniConstraint>(e =>
            {
                e.ToTable("GlobalUniConstraints");
                e.HasKey(guc => guc.ID);

                e.Property(p => p.PTeamSize)
                    .IsRequired();

                e.Property(p => p.SLoadCap)
                    .IsRequired();

                e.Property(p => p.ALoadCap)
                    .IsRequired();

                e.Property(p => p.PrefRankLimit)
                    .IsRequired();

                e.HasOne(guc => guc.University)
                    .WithMany()
                    .HasForeignKey(p => p.UniID)
                    .HasPrincipalKey(u => u.ID) // explicitly link to University.ID
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Session>(e =>
            {
                e.ToTable("Sessions");
                e.HasKey(s => s.ID);

                e.Property(s => s.Year)
                    .IsRequired();

                e.Property(s => s.SessionNo)
                    .IsRequired();

                e.Property(s => s.Dte_fr)
                    .IsRequired();

                e.Property(s => s.Dte_to)
                    .IsRequired();

                e.HasOne(s => s.University)
                    .WithMany()
                    .HasForeignKey(s => s.UniID)
                    .HasPrincipalKey(u => u.UniID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Course>(e =>
            {
                e.ToTable("Courses");
                e.HasKey(c => c.ID);
                e.HasAlternateKey(c => c.CourseID); // set CourseID as an alternate key

                e.Property(c => c.CourseName)
                    .HasMaxLength(255)
                    .IsRequired();

                e.Property(c => c.CourseCode)
                    .HasMaxLength(50)
                    .IsRequired();

                e.HasOne(c => c.Programs)
                    .WithMany()
                    .HasForeignKey(c => c.ProgramID)
                    .HasPrincipalKey(p => p.ProgramID);
            });

            modelBuilder.Entity<Programs>(e =>
            {
                e.ToTable("Programs");
                e.HasKey(p => p.ID);
                e.HasAlternateKey(p => p.ProgramID); // set ProgramID as an alternate key

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
                    .HasForeignKey(p => p.UniID)
                    .HasPrincipalKey(u => u.UniID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Module>(e =>
            {
                e.ToTable("Modules");
                e.HasKey(m => m.ID);
                e.HasAlternateKey(m => m.ModuleID); // set ModuleID as an alternate key

                e.Property(m => m.ModuleName)
                    .HasMaxLength(255)
                    .IsRequired();

                e.Property(m => m.ModuleCode)
                    .HasMaxLength(50)
                    .IsRequired();

                e.HasOne(m => m.Course)
                    .WithMany()
                    .HasForeignKey(m => m.CourseID)
                    .HasPrincipalKey(c => c.CourseID);
            });

            modelBuilder.Entity<FYPTopic>(e =>
            {
                e.ToTable("FYPTopics");
                e.HasKey(f => f.ID);
                e.HasAlternateKey(f => f.TopicID); // set TopicID as an alternate key

                e.Property(f => f.Program_Abbrev_Year_Session_IndexNo)
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(f => f.TopicTitle)
                    .HasMaxLength(255)
                    .IsRequired();

                e.Property(f => f.TopicDesc)
                    .HasColumnType("nvarchar(max)");

                e.Property(f => f.Tag)
                    .HasMaxLength(100);

                e.HasOne(f => f.Session)
                    .WithMany()
                    .HasForeignKey(f => f.SessionID);

                e.HasOne(f => f.Programs)
                    .WithMany()
                    .HasForeignKey(f => f.ProgramID)
                    .HasPrincipalKey(p => p.ProgramID);
            });

            modelBuilder.Entity<Preference>(e =>
            {
                e.ToTable("Preferences");
                e.HasKey(p => p.ID);

                e.Property(p => p.Rank)
                    .IsRequired();

                e.HasOne(p => p.User)
                    .WithMany(u => u.Preferences)
                    .HasForeignKey(p => p.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.FYPTopic)
                    .WithMany()
                    .HasForeignKey(p => p.TopicID)
                    .HasPrincipalKey(f => f.TopicID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Group>(e =>
            {
                e.ToTable("Groups");
                e.HasKey(g => g.ID);

                e.Property(g => g.GroupName)
                    .HasMaxLength(255)
                    .IsRequired();

                e.HasOne(g => g.FYPTopic)
                    .WithMany()
                    .HasForeignKey(g => g.TopicID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StaffModule>(e =>
            {
                e.ToTable("StaffModules");
                e.HasKey(sm => sm.ID);

                e.HasOne(sm => sm.StaffProfile)
                    .WithMany()
                    .HasForeignKey(sm => sm.StaffID)
                    .HasPrincipalKey(sp => sp.StaffID)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(sm => sm.Module)
                    .WithMany()
                    .HasForeignKey(sm => sm.ModuleID)
                    .HasPrincipalKey(m => m.ModuleID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StaffProfile>(e =>
            {
                e.ToTable("StaffProfiles");
                e.HasKey(sp => sp.ID);
                e.HasAlternateKey(sp => sp.StaffID); // set StaffID as an alternate key

                e.HasOne(sp => sp.User)
                    .WithOne(u => u.StaffProfile)
                    .HasForeignKey<StaffProfile>(sp => sp.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(sp => sp.StaffModules)
                    .WithOne(sm => sm.StaffProfile)
                    .HasForeignKey(sm => sm.StaffID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StudentProfile>(e =>
            {
                e.ToTable("StudentProfiles");
                e.HasKey(sp => sp.ID);
                e.HasAlternateKey(sp => sp.StudentID); // set StudentID as an alternate key

                e.Property(sp => sp.PhoneNo)
                    .IsRequired();

                e.Property(sp => sp.isFullTime)
                    .IsRequired();

                e.HasOne(sp => sp.User)
                    .WithOne(u => u.StudentProfile)
                    .HasForeignKey<StudentProfile>(sp => sp.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(sp => sp.Course)
                    .WithMany()
                    .HasForeignKey(sp => sp.CourseID)
                    .HasPrincipalKey(c => c.CourseID)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(sp => sp.Session)
                    .WithMany()
                    .HasForeignKey(sp => sp.SessionID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserRole>(e =>
            {
                e.ToTable("UserRoles");
                e.HasKey(ur => ur.ID);

                // Each UserRole has one User and one Role
                e.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID)
                .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleID)
                .OnDelete(DeleteBehavior.Restrict);
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
                    .IsRequired();

                // Optional relationships
                e.HasOne(f => f.University)
                    .WithMany()
                    .HasForeignKey(f => f.UniID)
                    .HasPrincipalKey(u => u.UniID)     // explicitly link to University.ID
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(f => f.Programs)
                    .WithMany()
                    .HasForeignKey(f => f.ProgramID)
                    .HasPrincipalKey(p => p.ProgramID)     // explicitly link to Programs.ID
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Assessment>(e =>
            {
                e.ToTable("Assessments");
                e.HasKey(a => a.ID);

                e.Property(e => e.MarkAwarded);

                e.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsRequired();

                e.Property(e => e.SubmittedAt);

                e.HasOne(a => a.Group)
                    .WithMany()
                    .HasForeignKey(a => a.GroupID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Tasks>(e =>
            {
                e.ToTable("Tasks");
                e.HasKey(t => t.ID);

                e.Property(t => t.TaskTitle)
                    .HasMaxLength(255)
                    .IsRequired();

                e.Property(t => t.TaskDesc)
                    .HasColumnType("nvarchar(max)");

                e.Property(t => t.DueAt)
                    .IsRequired();

                e.Property(t => t.Status)
                    .IsRequired();

                e.Property(t => t.SubmittedAt);

                e.Property(t => t.SubmittedBy)
                    .IsRequired(false);

                e.Property(t => t.FileName)
                    .IsRequired(false);

                e.HasOne(t => t.Group)
                    .WithMany()
                    .HasForeignKey(t => t.GroupID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TaskEvaluation>(e =>
            {
                e.ToTable("TaskEvaluations");
                e.HasKey(te => te.ID);

                e.Property(te => te.CriterionLabel)
                    .HasMaxLength(255)
                    .IsRequired();

                e.Property(te => te.Weight)
                    .IsRequired();

                e.Property(te => te.Score)
                    .IsRequired();

                e.Property(te => te.Weighted)
                    .IsRequired();

                e.Property(te => te.DeliverableScore)
                    .IsRequired();

                e.Property(te => te.FinalContribution)
                    .IsRequired();

                e.Property(te => te.Status)
                    .IsRequired();

                e.Property(te => te.SavedAt)
                    .IsRequired();

                e.HasOne(te => te.Task)
                    .WithMany()
                    .HasForeignKey(te => te.TaskID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserGroups>(e =>
            {
                e.ToTable("UserGroups");
                e.HasKey(ug => ug.ID);

                e.HasOne(ug => ug.User)
                    .WithMany(u => u.UserGroups)
                    .HasForeignKey(ug => ug.UserID)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(ug => ug.Group)
                    .WithMany(u => u.UserGroups)
                    .HasForeignKey(ug => ug.GroupID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UniSession>()
                .HasKey(us => new { us.UniID, us.Year, us.SessionID });

            modelBuilder.Entity<StudentModules>(e =>
            {
                e.ToTable("StudentModules");
                e.HasKey(sm => sm.ID);

                e.HasOne(sm => sm.Module)
                    .WithMany()
                    .HasForeignKey(sm => sm.ModuleID)
                    .HasPrincipalKey(m => m.ModuleID)  // ← ADD THIS LINE
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(sm => sm.StudentProfile)
                    .WithMany()
                    .HasForeignKey(sm => sm.StudentID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}