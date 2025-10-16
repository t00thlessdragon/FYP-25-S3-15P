using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("Course")]
    public class Course
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ProgramID { get; set; }

        [Required]
        public int CourseID { get; set; }

        [Required, StringLength(50)]
        public string CourseCode { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        // ✅ Update this line
        [ForeignKey("ProgramID")]
        public virtual UniversityProgram? Program { get; set; }
    }
}
