using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("TaskEvaluations", Schema = "dbo")]
    public class TaskEvaluation
    {
        [Key]
        public int ID { get; set; }

        [Required, StringLength(255)]
        [Column("CriterionLabel")]
        public string CriterionLabel { get; set; } = string.Empty;

        [Required]
        [Column("Weight")]
        public double Weight { get; set; }

        [Required]
        [Column("Score")]
        public double Score { get; set; }

        [Required]
        [Column("Weighted")]
        public double Weighted { get; set; }

        [Required]
        [Column("DeliverableScore")]
        public double DeliverableScore { get; set; }

        [Required]
        [Column("FinalContribution")]
        public double FinalContribution { get; set; }

        [Required]
        [Column("Status")]
        public string Status { get; set; } = string.Empty;

        [Required]
        [Column("SAvedAt")]
        public DateTime SavedAt { get; set; }

        // Foreign key
        [Required]
        public int TaskID { get; set; }

        // Navigation property
        public Tasks? Task { get; set; }
    }
}
