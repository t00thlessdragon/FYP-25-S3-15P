using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Index(nameof(ID), IsUnique = true)]
    [Table("Program", Schema = "dbo")]
    public class Programs
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Column("ProgramID")]
        public string ProgramID { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("ProgramName")]
        public string? ProgramName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("ProgramCode")]
        public string? ProgramCode { get; set; } = string.Empty;

        [ForeignKey(nameof(University))]
        [Required, StringLength(300)]
        [Column("UniID")]
        public string UniID { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [Required]
        public DateTime? CreatedAt { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Course? Course { get; set; }

        public University? University { get; set; }
    }
}
