namespace FYP_25_S3_15P.ViewModels
{
    public class UniversityMasterVm
    {
        public List<UniversityRow> Universities { get; set; } = new();

        public class UniversityRow
        {
            public int ID { get; set; }
            public string UniID { get; set; } = string.Empty;
            public string UniName { get; set; } = "";
            public string Address { get; set; } = string.Empty;
            public int ProgramCount { get; set; }
            public int UserCount { get; set; }
        }
    }
}
