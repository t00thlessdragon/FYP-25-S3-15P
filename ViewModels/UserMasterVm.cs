// ViewModels/UserMasterVm.cs
namespace FYP_25_S3_15P.ViewModels
{
    public class UserMasterVm
    {
        public List<Row> Users { get; set; } = new();

        public class Row
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
            public string UniName { get; set; } = "-";

            public int? RoleId { get; set; }       // optional (handy for future filters)
            public string Role { get; set; } = "-"; // ROLE NAME

            public string Status { get; set; } = "-";
            public bool IsLocked { get; set; }
            public DateTime? LastLogin { get; set; }

            public bool IsActive => !IsLocked;
            public string LastLoginLocal =>
                LastLogin.HasValue ? LastLogin.Value.ToLocalTime().ToString("g") : "-";
        }
    }
}