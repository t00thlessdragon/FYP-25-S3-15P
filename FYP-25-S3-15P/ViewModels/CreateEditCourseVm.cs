using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FYP_25_S3_15P.ViewModels
{
    public class CreateEditCourseVm
    {
        public int? ID { get; set; }

        [Required, StringLength(100)]
        public string CourseID { get; set; } = "";

        [Required, StringLength(255)]
        public string CourseName { get; set; } = "";

        [Required, StringLength(50)]
        public string CourseCode { get; set; } = "";

        [StringLength(500)]
        public string? Description { get; set; }

        public string ProgramID { get; set; } = "";

        // For dropdown
        public List<SelectListItem> Programs { get; set; } = new();
    }
}
