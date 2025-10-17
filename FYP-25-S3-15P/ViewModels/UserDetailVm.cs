namespace FYP_25_S3_15P.ViewModels
{
    public class UserDetailVm
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UniversityName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
