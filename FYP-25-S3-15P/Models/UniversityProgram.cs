using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("Program")]                // keep actual SQL table name
    public class UniversityProgram     // ✅ new class name
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int UniID { get; set; }

        [Required, MaxLength(50)]
        public string ProgramID { get; set; }

        [Required, MaxLength(50)]
        public string ProgramCode { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string ProgramName { get; set; } = string.Empty;

        [ForeignKey("UniID")]
        public virtual University? University { get; set; }

        public virtual ICollection<Course>? Courses { get; set; }
    }
}