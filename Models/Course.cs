

namespace FYP_25_S3_15P.Models
{
    public class Course
    {
        public int ID { get; set; }
        public int ProgramID { get; set; }

        // business key
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";

        // FK -> Program
        public Program Program { get; set; } = null!;

        // many-to-many to Module
        public ICollection<Module> Modules { get; set; } = new List<Module>();
    }
}
