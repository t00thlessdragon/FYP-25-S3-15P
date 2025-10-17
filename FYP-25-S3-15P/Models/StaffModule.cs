using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("StaffModules", Schema = "dbo")]
    public class StaffModule
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Column("StaffID")]
        public int StaffID { get; set; }

        [Required, StringLength(50)]
        [Column("ModuleID")]
        public string? ModuleID { get; set; }

        // Navigation properties
        public Module? Module { get; set; }
        public StaffProfile? StaffProfile { get; set; }
    }
}
