using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("Assessment", Schema = "dbo")]
    public class Assessment
    {
        [Key]
        public int ID { get; set; }

        [Column("MarkAwarded")]
        public int? MarkAwarded { get; set; } = 0;

        [Required, StringLength(50)]
        [Column("Status")]
        public string Status { get; set; } = string.Empty;

        [Column("SubmittedAt")]
        public DateTime? SubmittedAt { get; set; } = null;

        [ForeignKey(nameof(Group))]
        [Required]
        [Column("GroupID")]
        public int GroupID { get; set; }

        // Navigation properties
        public Group? Group { get; set; }
    }
}
