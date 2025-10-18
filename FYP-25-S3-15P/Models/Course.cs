using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Index(nameof(ID), IsUnique = true)]
    [Table("Course", Schema = "dbo")]
    public class Course
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(20)]
        [Column("CourseID")]
        [BindNever]
        public string CourseID { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        [Column("CourseName")]
        public string CourseName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("CourseCode")]
        public string CourseCode { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("ProgramID")]
        public string ProgramID { get; set; } = string.Empty;

        // Navigation properties
        public Programs? Programs { get; set; }
        public Module? Module { get; set; }
    }
}
