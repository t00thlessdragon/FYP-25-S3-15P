using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("AllocationPolicy")]
    public class AllocationPolicy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // (Optional) link to university
        public int? UniversityId { get; set; }

        // Session-specific policies
        public int? ProgramConstraintID { get; set; }

        // Hard constraints
        [Required]
        [Range(1, 20)]
        public int TeamSizePerGroup { get; set; } = 5;

        [Required]
        [Range(1, 20)]
        public int SupervisorLoadCap { get; set; } = 4;

        [Required]
        [Range(1, 20)]
        public int AssessorLoadCap { get; set; } = 4;

        [Required]
        [Range(1, 10)]
        public int PreferenceLimit { get; set; } = 3;

        // Scoring weights (soft preferences)
        public int Weight_Pref1 { get; set; } = 100;
        public int Weight_Pref2 { get; set; } = 60;
        public int Weight_Pref3 { get; set; } = 30;
        public int Weight_ModMatchSupervisor { get; set; } = 10;
        public int Weight_ModMatchAssessor { get; set; } = 10;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(ProgramConstraintID))]
        public ProgramConstraint? ProgramConstraint { get; set; }

        // Optional: allocation runs that used this policy
        public ICollection<AllocationRun>? AllocationRuns { get; set; }
    }
}
