using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("Universities", Schema = "dbo")]
    public class University
    {
        [Key]
        [Column("UniID")]
        public int UniID { get; set; }

        [Required, StringLength(200)]
        [Column("UniName")]
        public string UniName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("UEN")]
        public string UEN { get; set; } = string.Empty;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        public ICollection<ApplicationForm> ApplicationForms { get; set; } = new List<ApplicationForm>();
    }
}