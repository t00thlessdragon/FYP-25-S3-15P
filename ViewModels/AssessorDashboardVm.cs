using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class AssessorDashboardVm
    {
        public string AssessorName { get; set; } = "Assessor";
        public string? Search { get; set; }
        public List<ProjectRowVm> Projects { get; set; } = new();
    }
}
