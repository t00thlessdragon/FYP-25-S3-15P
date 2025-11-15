using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("PreferenceWeights")]
    public class PreferenceWeight
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ProgramConstraintID { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Rank must be between 1 and 10.")]
        public int RankNo { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Weight value must be between 0 and 1000.")]
        public int WeightValue { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(ProgramConstraintID))]
        public ProgramConstraint? ProgramConstraint { get; set; }
    }
}
