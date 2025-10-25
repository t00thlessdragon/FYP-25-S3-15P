namespace FYP_25_S3_15P.ViewModels
{
    public class AssessorTaskInfoVm
    {
        public string ProjectId   { get; set; } = "";   // e.g. CSIT-25-S3-08
        public string GroupId     { get; set; } = "";   // e.g. FYP-25-S3-15P
        public string TaskId      { get; set; } = "";   // e.g. T01
        public string Title       { get; set; } = "";   // e.g. Project requirement documentation
        public DateTime SubmissionDate { get; set; }
        public DateTime DueDate        { get; set; }
        public string Status      { get; set; } = "Submitted";
        public string FileName    { get; set; } = "";   // e.g. Project_Requirement_Doc.pdf
        public string? FileUrl    { get; set; }         // if you want to preview/download
    }
}
