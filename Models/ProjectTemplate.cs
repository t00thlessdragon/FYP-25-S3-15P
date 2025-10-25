using System.ComponentModel.DataAnnotations;

namespace FYP_25_S3_15P.Models
{
    public class ProjectTemplate
    {
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string TemplateCode { get; set; } = "";

        [Required, MaxLength(255)]
        public string Title { get; set; } = "";

        [Required, MaxLength(20)]
        public string ProgramCode { get; set; } = "";

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Active";

        public string? Description { get; set; }

        public ICollection<ProjectTemplateModule> TemplateModules { get; set; } = new List<ProjectTemplateModule>();
    }
}
