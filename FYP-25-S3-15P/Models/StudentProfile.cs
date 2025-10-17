using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("StudentProfile", Schema = "dbo")]
    public class StudentProfile
    {
        [Key]
        public int ID { get; set; }

        [Required, StringLength(50)]
        [Column("StudentID")]
        public int StudentID { get; set; }

        [ForeignKey(nameof(User))]
        [Required, StringLength(50)]
        [Column("UserID")]
        public int UserID { get; set; }

        [ForeignKey(nameof(Course))]
        [Required, StringLength(50)]
        [Column("CourseID")]
        public string? CourseID { get; set; }

        [Required, StringLength(50)]
        [Column("PhoneNo")]
        public string? PhoneNo { get; set; }

        [ForeignKey(nameof(Session))]
        [Required, StringLength(20)]
        [Column("SessionID")]
        public int SessionID { get; set; }

        [Required]
        [Column("isFullTime")]
        public bool isFullTime { get; set; }

        // Navigation properties
        public User? User { get; set; }

        public Course? Course { get; set; }

        public Session? Session { get; set; }
    }
}
