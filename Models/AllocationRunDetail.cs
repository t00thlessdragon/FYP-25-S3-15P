using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("AllocationRunDetails")]
    public class AllocationRunDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // FK → AllocationRuns
        [Required]
        public int RunId { get; set; }

        // FK → Users
        [Required]
        public int? UserId { get; set; }

        // FK → Groups
        [Required]
        public int GroupId { get; set; }

        // RoleId (Student / Supervisor / Assessor)
        [Required]
        public int RoleId { get; set; }

        // Weighted score assigned by the engine
        public double? Score { get; set; }

        // Preference rank (for students only)
        public int? PreferenceRank { get; set; }

        // Number of matched modules (for staff)
        public int? MatchedModules { get; set; }
        public string? ProjectId { get; set; }

        // Timestamp
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (optional but recommended)
        [ForeignKey(nameof(RunId))]
        public AllocationRun? AllocationRun { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [ForeignKey(nameof(GroupId))]
        public Group? Group { get; set; }
    }
}
