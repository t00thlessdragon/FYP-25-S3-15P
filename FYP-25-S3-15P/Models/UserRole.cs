using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("UserRole", Schema = "dbo")]
    public class UserRole
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey(nameof(User))]
        public int? UserID { get; set; }

        [ForeignKey(nameof(Role))]
        public int? RoleID { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Role? Role { get; set; }
    }
}
