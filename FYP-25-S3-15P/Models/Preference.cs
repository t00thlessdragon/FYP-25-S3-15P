using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("Preference")]
    public class Preference
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [Column("FYPTopics")]
        public string TopicID { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        [Column("Rank")]
        public string? Rank { get; set; } = string.Empty;

        // Navigation properties
        public User? User { get; set; }

        public FYPTopic? FYPTopic { get; set; }
    }
}
