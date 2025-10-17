using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("StaffProfile")]
    public class StaffProfile
    {
        [Key]
        public int ID { get; set; }

        [Required, StringLength(50)]
        public string StaffID { get; set; } = string.Empty;

        public int? UserID { get; set; }
        
        // 🆕 New Property for Session ID
        // This links the staff member to the universal session definition
        public int? CurrentSessionID { get; set; }

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 🔗 Relationships
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }

        [ForeignKey("UpdatedBy")]
        public virtual User? Updater { get; set; }

        [ForeignKey("CurrentSessionID")]
        public virtual Session? CurrentSession { get; set; } 
        
        // CRITICAL FIX: Missing Navigation Property for assigned modules.
        // EF Core needs this to load the modules when you use .Include(s => s.StaffModules)
        public virtual ICollection<StaffModules> StaffModules { get; set; } = new List<StaffModules>();
        // Note: 'StaffModules' must match the name of your join table model.
    }
}
