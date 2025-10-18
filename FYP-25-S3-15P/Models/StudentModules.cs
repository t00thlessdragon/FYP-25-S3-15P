using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("StudentModules", Schema = "dbo")]
    // Note: The table name in the database is StudentModules
    public class StudentModules
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        // --- Foreign Key to StudentProfile (The student who is taking the module) ---
        [Required]
        [ForeignKey("StudentProfile")]
        public int StudentID { get; set; }
        public StudentProfile? StudentProfile { get; set; }

        // --- Foreign Key to Module (The module being taken) ---
        [Required, MaxLength(50)]
        [ForeignKey("Module")]
        public string ModuleID { get; set; } = string.Empty;
        public Module? Module { get; set; }

        // --- Auditing Fields ---
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
        
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for the User who created the record (if needed)
        //[ForeignKey("CreatedBy")]
        //public User Creator { get; set; }
    }
}