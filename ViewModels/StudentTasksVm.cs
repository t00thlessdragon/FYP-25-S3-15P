namespace FYP_25_S3_15P.ViewModels
{
    public class StudentTasksVm
    {
        public List<StudentTaskRowVm> Tasks { get; set; } = new();
    }

    public class StudentTaskRowVm
    {
        public string TaskId { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime? DueDate { get; set; }
        public string Status { get; set; } = "";
    }
}
