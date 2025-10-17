namespace FYP_25_S3_15P.ViewModels
{
    public class ProgramMasterVm
    {
        public List<ProgramRow> Programs { get; set; } = new();

        public class ProgramRow
        {
            public int ID { get; set; }
            public string ProgramID { get; set; } = "";
            public string ProgramName { get; set; } = "";
            public string ProgramCode { get; set; } = "";
            public string Description { get; set; } = string.Empty;
            public string UniversityName { get; set; } = string.Empty;
            public string UniID { get; set; } = string.Empty;
            public int CourseCount { get; set; }
        }
    }
}
