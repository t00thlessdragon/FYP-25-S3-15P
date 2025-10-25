using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("StudentProfile")]
    public class StudentProfile
    {
        [Key]
        public int ID { get; set; }

        [Required, StringLength(50)]
        public string StudentID { get; set; } = string.Empty;

        public int? UserID { get; set; }

        public int? CourseID { get; set; }
        
        // --- New field for CSV Import (Kept from previous version) ---
        public int? YearOfStudy { get; set; }

        [StringLength(20)]
        public string? PhoneNo { get; set; }

        public int? SessionID { get; set; }

        public bool IsFullTime { get; set; } = false;

        // 🔗 Relationships
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("CourseID")]
        public virtual Course? Course { get; set; }

        // --- Session Relationship (Kept from previous version) ---
        [ForeignKey("SessionID")]
        public virtual Session? CurrentSession { get; set; }
        
        // --- Modules Navigation (Kept from previous version) ---
        public virtual ICollection<StudentModules> StudentModules { get; set; } = new List<StudentModules>();
    }
}
