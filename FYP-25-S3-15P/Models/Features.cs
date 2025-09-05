using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models;

[Table("features")]
public class Feature
{
    public int FeatureID { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }

    public ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();
}