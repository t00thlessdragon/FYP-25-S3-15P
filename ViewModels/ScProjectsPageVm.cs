using System;
using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class ScProjectsPageVm
    {
        public string ActiveTab { get; set; } = "projects";
        public int curYear { get; set; }
        public List<int> years { get; set; } = new();
        public List<ScConstraintRowVm> Constraints { get; set; } = new();
        public List<ProjectRow>        Projects    { get; set; } = new();
        public List<ScGroupRowVm> Groups { get; set; } = new();
        
        public class ProjectRow
        {
            public string Id { get; set; } = "";
            public string Title { get; set; } = "";
            public string Programme { get; set; } = "";
            public string Description { get; set; } = "";
            public bool IsPublish { get; set; }

            public List<string> Modules { get; set; } = new();

            public List<string> ModuleCodes { get; set; } = new();
        }
    }
}
