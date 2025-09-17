using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class ProjectRowVm
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public bool InPrefs { get; set; }
    }

    public class StudentDashboardVm
    {
        public string StudentName { get; set; } = "Jessy";
        public string StatusText { get; set; } = "Allocation Pending";
        public string ReminderText { get; set; } =
            "Please submit your first 3 project preferences before the deadline (28 July 2025).";
        public string? Search { get; set; }
        public List<ProjectRowVm> Projects { get; set; } = new();
    }
}
