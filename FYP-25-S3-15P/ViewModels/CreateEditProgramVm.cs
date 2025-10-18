using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FYP_25_S3_15P.ViewModels
{
    public class CreateEditProgramVm
    {
        public int? ID { get; set; }

        [BindNever]
        [Required, MaxLength(100)]
        public string ProgramID { get; set; } = "";

        [Required, MaxLength(255)]
        public string ProgramName { get; set; } = "";

        [Required, MaxLength(50)]
        public string ProgramCode { get; set; } = "";

        [MaxLength(500)]
        public string Description { get; set; } = "";

        [BindNever]
        public string UniID { get; set; } = "";

        // For dropdown
        public List<SelectListItem> Universities { get; set; } = new();
    }
}
