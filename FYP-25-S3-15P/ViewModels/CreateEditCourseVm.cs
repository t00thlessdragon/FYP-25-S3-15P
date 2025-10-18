using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FYP_25_S3_15P.ViewModels
{
    public class CreateEditCourseVm
    {
        public int? ID { get; set; }

        [BindNever]
        [Required, MaxLength(100)]
        public string CourseID { get; set; } = "";

        [Required, MaxLength(255)]
        public string CourseName { get; set; } = "";

        [BindNever]
        [Required, MaxLength(50)]
        public string CourseCode { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        public string ProgramID { get; set; } = "";

        // For dropdown
        public List<SelectListItem> Programs { get; set; } = new();
    }
}
