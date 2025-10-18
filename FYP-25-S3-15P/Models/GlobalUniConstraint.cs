using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FYP_25_S3_15P.Models
{
    [Table("GlobalUniConstraint", Schema = "dbo")]
    public class GlobalUniConstraint
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(50)]
        [Column("UniID")]
        public int UniID { get; set; }

        [Required, MaxLength(20)]
        [Column("PTeamSize")]
        public string? PTeamSize { get; set; }

        [Required, MaxLength(20)]
        [Column("SLoadCap")]
        public string? SLoadCap { get; set; }

        [Required, MaxLength(20)]
        [Column("ALoadCap")]
        public string? ALoadCap { get; set; }

        [Required, MaxLength(20)]
        [Column("PrefRankLimit")]
        public string? PrefRankLimit { get; set; }

        // Navigation properties
        public virtual University? University { get; set; }
    }
}
