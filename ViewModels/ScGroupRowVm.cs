namespace FYP_25_S3_15P.ViewModels
{
    public class ScGroupRowVm
    {
        public string ProjectId { get; set; } = "";
        public string Code { get; set; } = "";
        public string[] Members { get; set; } = System.Array.Empty<string>();
        public string Supervisor { get; set; } = "";
        public string Assessor { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
