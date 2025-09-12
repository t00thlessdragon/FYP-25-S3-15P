using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Data
{
    public class SmartDbContext : DbContext
    {
        public SmartDbContext(DbContextOptions<SmartDbContext> options) : base(options) { }

        // DbSets MUST be properties with get; set;  (do not "new()" them)
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = default!;
        public DbSet<Feature> Features { get; set; } = default!;
        public DbSet<PlanFeature> PlanFeatures { get; set; } = default!;
        public DbSet<ApplicationForm> ApplicationForms { get; set; } = default!;
        public DbSet<University>      Universities     { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubscriptionPlan>()
                .ToTable("subscriptionPlans")          // existing table name
                .HasKey(p => p.PlanID);

            modelBuilder.Entity<Feature>()
                .ToTable("Features")
                .HasKey(f => f.FeatureID);

            modelBuilder.Entity<PlanFeature>()
                .ToTable("PlanFeatures")               // change this to your actual table name
                .HasKey(pf => new { pf.PlanID, pf.FeatureID });

            modelBuilder.Entity<PlanFeature>()
                .HasOne(pf => pf.Plan)
                .WithMany(p => p.PlanFeatures)
                .HasForeignKey(pf => pf.PlanID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlanFeature>()
                .HasOne(pf => pf.Feature)
                .WithMany(f => f.PlanFeatures)
                .HasForeignKey(pf => pf.FeatureID)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ApplicationForm>(e =>
            {
                e.ToTable("ApplicationForm", "dbo");                     // singular
                e.HasOne(a => a.Plan)
                    .WithMany()
                    .HasForeignKey(a => a.PlanID);

                e.HasOne(a => a.University)
                    .WithMany(u => u.ApplicationForms)
                    .HasForeignKey(a => a.UniID);
            });

            modelBuilder.Entity<University>(e =>
            {
                e.ToTable("Universities", "dbo");
            });
        }
    }
}