using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FYP_25_S3_15P.Models
{
    [Table("University", Schema = "dbo")]
    public class University
    {
         [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string UniID { get; set; } = string.Empty;  // e.g. "200604346E"

        [Required]
        [MaxLength(255)]
        public string UniName { get; set; } = string.Empty;  // e.g. "National University of Singapore"

        [Required]
        [MaxLength(50)]
        public string UnivCode { get; set; } = string.Empty;  // e.g. "NUS"

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [Required]
        public DateTime? CreatedAt { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public ICollection<ApplicationForm> ApplicationForms { get; set; } = new List<ApplicationForm>();
    }
}