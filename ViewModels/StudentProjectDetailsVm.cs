namespace FYP_25_S3_15P.ViewModels
{
    public class StudentProjectDetailsVm
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; } = "";
        public string Title { get; set; } = "";
        public string? DescriptionLead { get; set; }    // first paragraph
        public List<string> Functionalities { get; set; } = new(); // bullet list
        public string? DescriptionTail { get; set; }    // optional last paragraph
        public List<string> Tags { get; set; } = new();
    }
}
