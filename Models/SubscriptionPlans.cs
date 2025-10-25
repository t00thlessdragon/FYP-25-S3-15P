using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models;

[Table("subscriptionPlans")]
public class SubscriptionPlan
{
    public int PlanID { get; set; }               // PK
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string? Description { get; set; }

    public ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();
}