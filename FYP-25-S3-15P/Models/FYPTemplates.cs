using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("FYPTemplates", Schema = "dbo")]
    public class FYPTemplates
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(50)]
        public string UniID { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string ProgramID { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string ProjectName { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public University? University { get; set; }

        public Programs? Programs { get; set; }
    }
}
