// ViewModels/AssessorAssessmentRowVm.cs
namespace FYP_25_S3_15P.ViewModels
{
    public class AssessorAssessmentRowVm
    {
        public int No { get; set; }
        public string GroupId { get; set; } = "";
        public string ProjectId { get; set; } = "";
        public string ProjectTitle { get; set; } = "";
        public string Status { get; set; } = "";        // "Pending" / "Submitted"
        public DateTime SubmissionDate { get; set; }
        public string MarksAwarded { get; set; } = "";  // e.g. "0%" / "80%"
    }

    public class SubmitAssessmentTaskRowVm
    {
        public string TaskId { get; set; } = "";
        public string TaskTitle { get; set; } = "";
        public int ScoreOutOf100 { get; set; }          // e.g. 80
        public decimal ContributionPercent { get; set; } // e.g. 10
        public decimal FinalContribution => Math.Round(ScoreOutOf100 * (ContributionPercent / 100m), 2); // not displayed directly, for convenience
    }

    public class SubmitAssessmentVm
    {
        public string ProjectId { get; set; } = "";
        public string ProjectTitle { get; set; } = "";
        public string GroupId { get; set; } = "";
        public string Supervisor { get; set; } = "";
        public string Assessor { get; set; } = "";
        public List<string> Members { get; set; } = new();
        public List<SubmitAssessmentTaskRowVm> Tasks { get; set; } = new();
        public decimal TotalContributionPercent { get; set; } // e.g. 19.25
    }
}
