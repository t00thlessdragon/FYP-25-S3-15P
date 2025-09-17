using System.Collections.Generic;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.ViewModels
{
    public class HomeLandingVm
    {
        public List<SubscriptionPlan> Plans { get; set; } = new();
        public List<Feature> HomeFeatures { get; set; } = new();
        public IEnumerable<FAQ> FAQs { get; set; } = new List<FAQ>();
    }
}