using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FYP_25_S3_15P.ViewModels
{
    public class CreateEditProgramVm
    {
        public int? ID { get; set; }

        [Required, StringLength(100)]
        public string ProgramID { get; set; } = "";

        [Required, StringLength(255)]
        public string ProgramName { get; set; } = "";

        [Required,StringLength(50)]
        public string ProgramCode { get; set; } = "";

        [StringLength(500)]
        public string Description { get; set; } = "";

        public string UniID { get; set; } = "";

        // For dropdown
        public List<SelectListItem> Universities { get; set; } = new();
    }
}
