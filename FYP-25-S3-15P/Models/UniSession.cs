using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // Needed for composite key setup

namespace FYP_25_S3_15P.Models
{
    // Maps to the [dbo].[UniSession] table: UniID, Year, SessionID, Dates
    [Table("UniSession", Schema = "dbo")]
    public class UniSession
    {
        // 🔑 Primary Keys (These are also used for the composite key configuration in DbContext)
        [Column("UniID")]
        public int UniID { get; set; }

        [Column("Year")]
        public int Year { get; set; }

        [Column("SessionID")]
        public int SessionID { get; set; }

        [Required]
        [Column("Date_Frm")]
        public DateTime DateFrom { get; set; }

        [Required]
        [Column("Date_To")]
        public DateTime DateTo { get; set; }

        // 🔗 Navigation Properties
        [ForeignKey("UniID")]
        public virtual University? University { get; set; }

        [ForeignKey("SessionID")]
        public virtual Session? Session { get; set; }
    }
}