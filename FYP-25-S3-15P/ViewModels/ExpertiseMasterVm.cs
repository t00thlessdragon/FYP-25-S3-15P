namespace FYP_25_S3_15P.ViewModels
{
    public class ExpertiseMasterVm
    {
        public List<ExpertiseRow> Experts { get; set; } = new();

        public class ExpertiseRow
        {
            public int ID { get; set; }
            public string StaffName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Modules { get; set; } = string.Empty;
            public int ModuleCount { get; set; }
        }
    }
}
