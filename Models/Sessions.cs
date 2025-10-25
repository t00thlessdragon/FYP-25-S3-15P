using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("Sessions", Schema = "dbo")]
    public class Session
    {
        [Key]
        [Column("SessionID")]
        public int SessionID { get; set; }

        [Required]
        [Column("SessionName"), StringLength(50)]
        public string SessionName { get; set; } = string.Empty;
    }
}