using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FYP_25_S3_15P.ViewModels
{
    public class CreateEditUserVm
    {
        public int? ID { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = "";

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = "";

        [MaxLength(256)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        public int RoleID { get; set; }

        [BindNever]
        public string UniID { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Active";

        public bool IsLocked { get; set; }
        public bool MustChangePassword { get; set; } = true;

        // For dropdowns
        public List<SelectListItem> Roles { get; set; } = new();
        public List<SelectListItem> Universities { get; set; } = new();
    }
}
