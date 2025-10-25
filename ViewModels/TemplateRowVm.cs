// ViewModels/TemplateRowVm.cs
namespace FYP_25_S3_15P.ViewModels
{
    public class TemplateRowVm
    {
        public string TemplateId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Programme { get; set; } = "";       
        public List<string> Modules { get; set; } = new();
        public string Status { get; set; } = "Active";
        public string Description { get; set; } = "";
    }
}
