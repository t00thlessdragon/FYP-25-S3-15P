using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("ProjectModules")] 
    public class ProjectModule
    {
        [Key, Column(Order = 0)]
        public string ProjectId { get; set; } = "";

        [Key, Column(Order = 1)]
        public string ModuleCode { get; set; } = "";

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; } = null!;

        [ForeignKey(nameof(ModuleCode))]
        public Module Module { get; set; } = null!;
    }
}
