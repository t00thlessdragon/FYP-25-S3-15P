using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    public class StaffModules
    {
        [Key]
        public int ID { get; set; }

        public int? StaffID { get; set; }
        [ForeignKey("StaffID")]
        public StaffProfile Staff { get; set; }

        public int? ModuleID { get; set; }
        [ForeignKey("ModuleID")]
        public Module Module { get; set; }

        public int? CreatedBy { get; set; }
        [ForeignKey("CreatedBy")]
        public User Creator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
