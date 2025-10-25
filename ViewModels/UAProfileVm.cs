using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class UAProfileVm
    {
        // Header
        public string AvatarUrl { get; set; } = "/images/default-avatar.png";
        public string Role { get; set; } = "University Admin";

        // University Admin Details (non-editable)
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string StaffID { get; set; } = "";

        // University Details
        public string UniName { get; set; } = "";
        public int TotalStudents { get; set; }
        public int TotalStaff { get; set; }
        public int TotalAssessors { get; set; }
        public int TotalSupervisors { get; set; }
        public int TotalSubjectCoordinators { get; set; }
        public int TotalPrograms { get; set; }
        public int TotalCourses { get; set; }
        public int TotalModules { get; set; }
    }
}