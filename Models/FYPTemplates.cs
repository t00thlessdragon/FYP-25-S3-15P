using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("FYPTemplates")]  // ✅ matches your DB table name
    public class FYPTemplates
    {
        [Key]
        public int ID { get; set; }

        // Foreign Key to University
        public int UniID { get; set; }

        // Foreign Key to Program (if you have a Program/Prog table)
        public int ProgID { get; set; }

        [Required]
        [MaxLength(255)]
        public string ProjectName { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Description { get; set; }

        [MaxLength(100)]
        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // --- Optional Navigation Properties ---
        [ForeignKey(nameof(UniID))]
        public virtual University? University { get; set; }

        [ForeignKey(nameof(ProgID))]
        public virtual Programs? Programs { get; set; }
    }
}