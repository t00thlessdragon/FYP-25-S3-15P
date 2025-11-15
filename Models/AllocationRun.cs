using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("AllocationRuns")]
    public class AllocationRun
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProgramConstraintID { get; set; }

        // Execution info
        [Required]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? FinishedAt { get; set; }

        [MaxLength(50)]
        public string Strategy { get; set; } = "GreedyWeighted";

        [MaxLength(20)]
        public string Status { get; set; } = "Completed"; // Completed / Partial / Failed

        public string? Notes { get; set; }

        // Metrics
        public double? AvgPreferenceScore { get; set; }

        public double? AvgSupervisorLoad { get; set; }

        public double? AvgAssessorLoad { get; set; }

        public int UnassignedStudents { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        // Related allocation details
        public ICollection<AllocationRunDetail>? AllocationRunDetails { get; set; }
    }
}
