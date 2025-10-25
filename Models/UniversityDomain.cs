using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FYP_25_S3_15P.Models
{
    [Table("UniversityDomains")]
    [Index(nameof(DomainNormalized), IsUnique = true)]
    public class UniversityDomain
    {
        [Key] public int DomainId { get; set; }

        [Required] public int UniID { get; set; }          // FK → Universities
        [Required, StringLength(255)] public string Domain { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Computed in SQL: LOWER(Domain)
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [StringLength(255)]
        public string? DomainNormalized { get; private set; }

        [ForeignKey(nameof(UniID))] public University? University { get; set; }
    }
}