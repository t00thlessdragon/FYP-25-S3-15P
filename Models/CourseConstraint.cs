using System;

namespace FYP_25_S3_15P.Models
{
    // Per-course override or copy from UniversityConstraint
    public class CourseConstraint
    {
        public int Id { get; set; }
        public int UniID { get; set; }
        public int Year { get; set; }
        public int CourseId { get; set; }   
        public int SessionNo { get; set; }  

        public int SLoadCap { get; set; }
        public int ALoadCap { get; set; }
        public int PrefRankLimit { get; set; }
        public int SMin { get; set; }
        public int SMax { get; set; }

        public bool IsOverride { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Course Course { get; set; } = null!;
    }
}
