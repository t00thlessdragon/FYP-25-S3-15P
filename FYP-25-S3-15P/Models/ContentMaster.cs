using System.Collections.Generic;

namespace FYP_25_S3_15P.Models;

public class ContentMaster
{
    public string ActiveTab { get; set; } = "plans";
    public List<SubscriptionPlan> Plans { get; set; } = new();
    public List<Feature> Features { get; set; } = new();
}