using Microsoft.EntityFrameworkCore;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Data;

public class SmartDbContext : DbContext
{
    public SmartDbContext(DbContextOptions<SmartDbContext> options) : base(options)
    {
    }

    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<PlanFeature>().HasKey(x => new { x.PlanID, x.FeatureID });

        b.Entity<PlanFeature>()
            .HasOne(x => x.Plan)
            .WithMany(p => p.PlanFeatures)
            .HasForeignKey(x => x.PlanID);

        b.Entity<PlanFeature>()
            .HasOne(x => x.Feature)
            .WithMany(f => f.PlanFeatures)
            .HasForeignKey(x => x.FeatureID);
    }
}