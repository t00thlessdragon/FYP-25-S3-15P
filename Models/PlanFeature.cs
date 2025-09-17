namespace FYP_25_S3_15P.Models;

public class PlanFeature
{
    public int PlanID { get; set; }
    public SubscriptionPlan Plan { get; set; } = null!;

    public int FeatureID { get; set; }
    public Feature Feature { get; set; } = null!;
}
