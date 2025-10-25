using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models;

[Table("features")]
public class Feature
{
    public int FeatureID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();

    // NEW: fields for home-page cards
    public bool ShowOnHome { get; set; }              // default false
    public int? HomeOrder { get; set; }               // lower = earlier
    public string? HomeTitle { get; set; }
    public string? HomeSummary { get; set; }
    public string? HomeImagePath { get; set; }        // e.g. "/images/features/abc.jpg"
}