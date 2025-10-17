namespace FYP_25_S3_15P.ViewModels
{
    public class ExpertiseDetailVm
    {
        public int StaffProfileID { get; set; } = 0;
        public int UserID { get; set; } = 0;
        public string StaffName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<ModuleItem> Modules { get; set; } = new();

        public class ModuleItem
        {
            public string ModuleID { get; set; } = string.Empty;
            public string ModuleName { get; set; } = string.Empty;
            public string CourseName { get; set; } = string.Empty;
        }
    }
}
