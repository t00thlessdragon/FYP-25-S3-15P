using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Index(nameof(ID), IsUnique = true)]
    [Table("Session", Schema = "dbo")]
    public class Session
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(50)]
        [Column("UniID")]
        public string UniID { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("Year")]
        public string Year { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("SessionNo")]
        public string SessionNo { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("Dte_fr")]
        public string Dte_fr { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Column("Dte_to")]
        public string Dte_to { get; set; } = string.Empty;

        // Navigation properties
        public virtual University? University { get; set; }
    }
}
