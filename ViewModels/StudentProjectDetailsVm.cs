using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class StudentProjectDetailsVm
    {
        public string Id { get; set; } = "";           
        public string ProjectCode { get; set; } = "";
        public string Title { get; set; } = "";
        public string? DescriptionLead { get; set; }
        public List<string> Functionalities { get; set; } = new();
        public string? DescriptionTail { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
