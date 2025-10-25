// Models/Project.cs
using System.Collections.Generic;

namespace FYP_25_S3_15P.Models
{
    public class Project
    {
        // e.g., CSIT-25-S1-01
        public string ProjectId { get; set; } = "";
        public string Title { get; set; } = "";
        public string ProgramCode { get; set; } = "";
        public string CourseCode { get; set; } = "";
        public int Year { get; set; }
        public int SessionNo { get; set; }      // 1,2,3
        public string? Description { get; set; }
        public DateTime? PrefCloseAt { get; set; }
        public bool IsPublish { get; set; }
     

        public ICollection<ProjectModule> ProjectModules { get; set; } = new List<ProjectModule>();
    }
}
