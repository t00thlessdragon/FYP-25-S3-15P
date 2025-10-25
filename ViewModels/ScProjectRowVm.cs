using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public class ScProjectRowVm
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Programme { get; set; } = "";
        public List<string> Modules { get; set; } = new();
        public string Description { get; set; } = "";
    }
}
