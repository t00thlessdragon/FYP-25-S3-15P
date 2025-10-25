using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("ApplicationForm", Schema = "dbo")]            // map to singular table
    public class ApplicationForm
    {
        [Key]
        [Column("AppId")]                                  // PK = AppId
        public int AppId { get; set; }

        [Required, StringLength(120)]
        [Column("ApplicantName")]
        public string ApplicantName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(254)]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("Role")]
        public string Role { get; set; } = string.Empty;    // free text from form

        [Column("PlanID")]
        public int PlanID { get; set; }
        public SubscriptionPlan? Plan { get; set; }

        [Column("UniID")]
        public int UniID { get; set; }
        public University? University { get; set; }

        [StringLength(20)]
        [Column("Status")]
        public string Status { get; set; } = "Pending";

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}