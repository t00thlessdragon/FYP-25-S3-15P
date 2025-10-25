namespace FYP_25_S3_15P.ViewModels
{
    public class AssessorTaskDeliverableRowVm
    {
        public int No { get; set; }
        public string GroupId { get; set; } = "";
        public string ProjectId { get; set; } = "";   // e.g. CSIT-25-S3-08
        public string ProjectTitle { get; set; } = "";
        public string TaskId { get; set; } = "";      // e.g. T01
        public string TaskTitle { get; set; } = "";
        public DateTime SubmittedOn { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "";      // e.g. “Evaluation Saved”, “Pending Evaluation”
        public string? FileUrl { get; set; }          // for Download button
    }
}
