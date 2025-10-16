using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("Programs")]  // ✅ matches DB table name
    public class Programs
    {
        [Key]
        public int ID { get; set; }

        // Nullable unique identifier for program
        public int? ProgramID { get; set; }

        [Required]
        [MaxLength(255)]
        public string ProgramName { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProgramCode { get; set; }

        // Foreign key to University
        [Required]
        public int UniID { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [Required]
        public DateTime? CreatedAt { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // --- Optional Navigation Property ---
        [ForeignKey(nameof(UniID))]
        public virtual University University { get; set; }
    }
}
