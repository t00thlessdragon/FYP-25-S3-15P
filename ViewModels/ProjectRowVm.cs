using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class ProjectRowVm
    {
        public string Id { get; set; } = "";          
        public string ProjectCode { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public bool InPrefs { get; set; }
    }
}
