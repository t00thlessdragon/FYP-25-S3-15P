using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FYP_25_S3_15P.ViewModels
{
    public class RubricItemVm
    {
        [Required] public string Criterion    { get; set; } = "";
        public string Description             { get; set; } = "";
        [Range(0, 100)] public int Weight     { get; set; }   // % (e.g., 50)
        [Range(0, 10)]  public int Score      { get; set; }   // 0..10
        public decimal Weighted => Math.Round(Score * (Weight / 10m), 1); // auto
    }

    public class AssessorEvaluateVm
    {
        // header
        public string ProjectId { get; set; } = "";
        public string TaskId    { get; set; } = "";
        public string Title     { get; set; } = "";
        public string GroupId   { get; set; } = "";
        public DateTime SubmittedOn { get; set; }

        // how much this deliverable contributes to overall project grade (e.g., 10%)
        public int DeliverableWeightPercent { get; set; } = 10;

        public List<RubricItemVm> Items { get; set; } = new();

        // read-only helpers
        public decimal TotalScore =>
            Math.Round(Items.Sum(i => i.Score * (i.Weight / 10m)), 1); // out of 100

        public decimal ContributionPercent =>
            Math.Round((TotalScore / 100m) * DeliverableWeightPercent, 1);
    }
}
