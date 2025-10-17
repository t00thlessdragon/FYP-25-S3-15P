namespace FYP_25_S3_15P.ViewModels
{
    public class CourseMasterVm
    {
        public List<CourseRow> Courses { get; set; } = new();

        public class CourseRow
        {
            public int ID { get; set; }
            public string CourseID { get; set; } = "";
            public string CourseName { get; set; } = "";
            public string Description { get; set; } = "";
            public string ProgramName { get; set; } = "";
            public string ProgramID { get; set; } = "";
            public int ModuleCount { get; set; }
        }
    }
}
