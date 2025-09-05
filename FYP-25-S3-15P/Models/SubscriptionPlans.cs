using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models;

[Table("subscriptionPlans")]
public class SubscriptionPlan
{
    public int PlanID { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? FeaturesText { get; set; }   // optional legacy text

    public ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();
}