namespace FYP_25_S3_15P.Models
{
    public class Module
    {
        public int ID { get; set; }

        // business key
        public string ModuleCode { get; set; } = "";
        public string ModuleName { get; set; } = "";
       

        // many-to-many to Course (REMOVE any old CourseID/Course navs)
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
