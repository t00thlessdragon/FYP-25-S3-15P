// Models/ProjectTemplateModule.cs
namespace FYP_25_S3_15P.Models
{
    public class ProjectTemplateModule
    {
        public int ProjectTemplateId { get; set; }
        public string ModuleCode { get; set; } = "";          

        // navs (optional)
        public ProjectTemplate ProjectTemplate { get; set; } = null!;
        public Module Module { get; set; } = null!;
    }
}
