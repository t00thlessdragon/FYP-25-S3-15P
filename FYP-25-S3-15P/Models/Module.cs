using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Index(nameof(ID), IsUnique = true)]
    [Table("Module", Schema = "dbo")]
    public class Module
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(50)]
        [Column("ModuleID")]
        public string ModuleID { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("ModuleName")]
        public string ModuleName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        [Column("ModuleCode")]
        public string ModuleCode { get; set; } = string.Empty;

        [ForeignKey(nameof(Course))]
        [Required, MaxLength(20)]
        [Column("CourseID")]
        public string CourseID { get; set; } = string.Empty;

        // Navigation properties
        public Course? Course { get; set; }
    }
}
