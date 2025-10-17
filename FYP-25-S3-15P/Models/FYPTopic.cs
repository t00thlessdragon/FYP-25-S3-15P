using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("FYPTopics", Schema = "dbo")]
    public class FYPTopic
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string TopicID { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Program_Abbrev_Year_Session_IndexNo { get; set; }

        [StringLength(100)]
        public string? TopicTitle { get; set; }

        [StringLength(500)]
        public string? TopicDesc { get; set; }

        [StringLength(50)]
        public string? Tag { get; set; }

        public int? SessionID { get; set; }

        public int? ProgramID { get; set; }

        // Navigation properties
        public University? University { get; set; }
        public Session? Session { get; set; }
        public Programs? Programs { get; set; }
        public ICollection<FYPTemplates> FYPTemplates { get; set; } = new List<FYPTemplates>();
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}
