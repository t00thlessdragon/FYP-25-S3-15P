using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FYP_25_S3_15P.Models
{
    public class Program
    {
        public int ID { get; set; }
        public int UniID { get; set; }

        // business key
        public string ProgramCode { get; set; } = "";
        public string ProgramName { get; set; } = "";

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
