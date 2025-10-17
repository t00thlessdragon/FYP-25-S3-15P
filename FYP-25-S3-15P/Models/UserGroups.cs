using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("UserGroups", Schema = "dbo")]
    public class UserGroups
    {
        [Key]
        public int ID { get; set; }

        public int? UserID { get; set; }

        public int? GroupID { get; set; }

        public int? RoleID { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Group? Group { get; set; }
        public Role? Role { get; set; }
    }
}
