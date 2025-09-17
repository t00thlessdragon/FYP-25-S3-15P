namespace FYP_25_S3_15P.ViewModels
{
    public class StudentProfileVm
    {
        // Header
        public string Name { get; set; } = "Jessy";
        public string Role { get; set; } = "Student";
        public string AvatarUrl { get; set; } = "/images/avatar-student.png";

        // Editable
        public string? Email { get; set; } = "jessy@gmail.com";
        public string? Phone { get; set; }

        // Read-only display
        public int StudentId { get; set; } = 8743567;
        public string Course { get; set; } = "Bachelor of Computer Science";
        public int YearOfStudy { get; set; } = 3;
        public string Program { get; set; } = "CSIT 321 Project";
        public string ProjectGroup { get; set; } = "FYP-25-S3-15P";
        public string Supervisor { get; set; } = "Mr.Prem";
        public string Assessor { get; set; } = "Mr.Tan";
        public string ProjectTitle { get; set; } = "A Smart Project Allocation System";
        public string University { get; set; } = "Singapore Institute of Management";
    }
}
