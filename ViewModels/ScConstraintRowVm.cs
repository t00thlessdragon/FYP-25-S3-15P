using System;

namespace FYP_25_S3_15P.ViewModels
{
    public class ScConstraintRowVm
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int Session { get; set; }
        public string Status { get; set; } = "Default";
        public int Year { get; set; }
        public DateTime Updated { get; set; }
    }
}
