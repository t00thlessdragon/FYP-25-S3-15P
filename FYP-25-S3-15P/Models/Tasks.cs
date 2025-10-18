using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("Tasks", Schema = "dbo")]
    public class Tasks
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int GroupID { get; set; }

        [Required, MaxLength(255)]
        [Column("TaskTitle")]
        public string TaskTitle { get; set; } = string.Empty;

        [Required]
        [Column("TaskDesc")]
        public string TaskDesc { get; set; } = string.Empty;

        [Required]
        [Column("DueAt")]
        public DateTime? DueAt { get; set; } = null;

        [Required, MaxLength(50)]
        [Column("Status")]
        public string Status { get; set; } = string.Empty;
        
        [Column("SubmittedAt")]
        public DateTime? SubmittedAt { get; set; }

        [Column("SubmittedBy")]
        public string SubmittedBy { get; set; } = string.Empty;

        [Column("FileName")]
        public string FileName { get; set; } = string.Empty;

        // Navigation property
        public Group? Group { get; set; }
    }
}
