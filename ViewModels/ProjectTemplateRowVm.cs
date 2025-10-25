namespace FYP_25_S3_15P.ViewModels
{
    public class ProjectTemplateRowVm
    {
        public string TemplateId { get; set; } = "";   
        public string Title { get; set; } = "";
        public string Programme { get; set; } = "";    
        public string[] Modules { get; set; } = [];    
        public string Status { get; set; } = "Active";
        public string? Description { get; set; }
    }
}
