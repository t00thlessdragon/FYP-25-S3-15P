using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Index(nameof(UniID), IsUnique = true)]
    [Table("University", Schema = "dbo")]
    public class University
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(50)]
        [Column("UniID")]
        public string UniID { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        [Column("UniName")]
        public string UniName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("UnivCode")]
        public string UnivCode { get; set; } = string.Empty;

        public ICollection<ApplicationForm> ApplicationForms { get; set; } = new List<ApplicationForm>();
        public ICollection<Programs> Programs { get; set; } = new List<Programs>();
        public ICollection<FYPTopic> FYPTopics { get; set; } = new List<FYPTopic>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}