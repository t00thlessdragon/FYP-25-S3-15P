namespace FYP_25_S3_15P.ViewModels
{
    public class UADashboardVm
    {
        public string AdminName { get; set; } = string.Empty;
        public string UniversityName { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int TotalPrograms { get; set; }
        public int TotalCourses { get; set; }
        public int TotalExperts { get; set; }
    }
}
