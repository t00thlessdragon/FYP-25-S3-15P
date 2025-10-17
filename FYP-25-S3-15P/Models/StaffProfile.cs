using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("StaffProfile", Schema = "dbo")]
    public class StaffProfile
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [Column("StaffID")]
        public int StaffID { get; set; }

        [ForeignKey(nameof(User))]
        [Required]
        [Column("UserID")]
        public int UserID { get; set; }

        public List<StaffModule> StaffModules { get; set; } = new List<StaffModule>();

        // Navigation properties
        public User? User { get; set; }
    }
}
