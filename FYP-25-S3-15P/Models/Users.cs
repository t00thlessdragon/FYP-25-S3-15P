// Models/User.cs
using FYP_25_S3_15P.ViewModels;
using FYP_25_S3_15P.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    // If you prefer uniqueness per-university instead of platform-wide,
    // change the Index to: [Index(nameof(EmailNormalized), nameof(UniID), IsUnique = true)]
    [Index(nameof(EmailNormalized), IsUnique = true)]
    [Table("Users")]
    public class User
    {
        [Key]
        public int ID { get; set; }

        // --- Relationships ---
        [Required]
        [ForeignKey(nameof(University))]
        public string UniID { get; set; } = string.Empty;              // FK -> University.UniID

        [Required]
        public int RoleID { get; set; }             // FK -> Roles.ID

        // --- Core profile ---
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        // Persisted computed column in DB: LOWER(Email)
        [StringLength(256)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? EmailNormalized { get; private set; }

        // Store a *hashed* password (never plaintext)
        [Required, StringLength(256)]
        public string Password { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Status { get; set; } = "Active"; // Active / Disabled, etc.

        public bool MustChangePassword { get; set; } = true;
        public bool IsLocked { get; set; } = false;

        public DateTime? LastLogin { get; set; }

        // --- Auditing ---
        public int? CreatedBy { get; set; }          // FK to Users.Id (creator)
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Rowversion for optimistic concurrency
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // --- Navigation properties ---
        public Role? Role { get; set; }

        public University? University { get; set; }

        public User? Users { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [StringLength(255)]
        public string? EmailDomain { get; private set; }

        public StudentProfile? StudentProfile { get; set; }
        public StaffProfile? StaffProfile { get; set; }
        public virtual ICollection<UserGroups> UserGroups { get; set; } = new List<UserGroups>();
        public virtual ICollection<Preference> Preferences { get; set; } = new List<Preference>();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
