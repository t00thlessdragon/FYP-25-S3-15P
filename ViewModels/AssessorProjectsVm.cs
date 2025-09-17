// ViewModels/AssessorProjectsVm.cs
namespace FYP_25_S3_15P.ViewModels
{
    /// Represents one row in the Assessor Project Listing table.
    public class AssessorProjectsVM
    {
        public int No { get; set; }                     // 1,2,3...
        public string ProjectId { get; set; } = "";     // e.g. CSIT-25-S3-08
        public string ProjectTitle { get; set; } = "";  // title
        public string GroupId { get; set; } = "";       // e.g. FYP-25-S3-15P
        public string Supervisor { get; set; } = "";    // e.g. Mr Prem
        public string Status { get; set; } = "";        // Active / Completed
        public int ProjectKey { get; set; }             // internal id for routing
    }
}
