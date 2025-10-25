using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class AssessorProjectDetailsVm
    {
        public string ProjectId { get; set; } = "";
        public string Title { get; set; } = "";
        public string GroupId { get; set; } = "";
        public string Supervisor { get; set; } = "";
        public string Assessor { get; set; } = "";
        public List<string> Members { get; set; } = new();
        public string Description { get; set; } = "";
    }
}
