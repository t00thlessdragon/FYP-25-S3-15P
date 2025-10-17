using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Index(nameof(ID), IsUnique = true)]
    [Table("Groups", Schema = "dbo")]
    public class Group
    {
        [Key]
        public int ID { get; set; }

        [StringLength(500)]
        public string? GroupName { get; set; }

        public bool? IsFullTime { get; set; }

        public int TopicID { get; set; }

        // Navigation properties
        public FYPTopic? FYPTopic { get; set; }
        public ICollection<UserGroups> UserGroups { get; set; } = new List<UserGroups>();
    }
}
