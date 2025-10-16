using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    public class Module
    {
        [Key]
        public int ID { get; set; }

        public int ModuleID { get; set; }
        [Required, MaxLength(50)]
        public string ModuleCode { get; set; }
        [Required, MaxLength(200)]
        public string ModuleName { get; set; }

        public List<StaffModules> StaffModules { get; set; } = new List<StaffModules>();
    }
}
