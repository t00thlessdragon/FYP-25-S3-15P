using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("StudentPreferences")]
    public class StudentPreference
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // FK → Users (not StudentProfile)
        [Required]
        public int StudentId { get; set; }

        // Project or Topic code (string)
        [Required]
        [MaxLength(100)]
        public string ProjectId { get; set; } = string.Empty;

        // Rank (1 = top preference)
        [Required]
        [Range(1, 10)]
        public int Rank { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedAtUtc { get; set; }

        // Navigation property to Users table
        [ForeignKey(nameof(StudentId))]
        public User? Student { get; set; }
    }
}
