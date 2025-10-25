using System;

namespace FYP_25_S3_15P.Models
{
    public class UniversityConstraint
    {
        public int Id { get; set; }
        public int UniID { get; set; }
        public int Year { get; set; }

        public int SLoadCap { get; set; }         
        public int ALoadCap { get; set; }         
        public int PrefRankLimit { get; set; }
        public int SessionNo { get; set; }        
        public int SMin { get; set; }             
        public int SMax { get; set; }            

        public string?  S1Label { get; set; }
        public DateTime? S1_Dte_fr { get; set; }
        public DateTime? S1_Dte_to { get; set; }

        public string?  S2Label { get; set; }
        public DateTime? S2_Dte_fr { get; set; }
        public DateTime? S2_Dte_to { get; set; }

        public string?  S3Label { get; set; }
        public DateTime? S3_Dte_fr { get; set; }
        public DateTime? S3_Dte_to { get; set; }

        public string?  S4Label { get; set; }
        public DateTime? S4_Dte_fr { get; set; }
        public DateTime? S4_Dte_to { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
