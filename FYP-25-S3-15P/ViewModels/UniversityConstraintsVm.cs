namespace FYP_25_S3_15P.ViewModels
{
    public class UniversityConstraintsVm
    {
        public List<ConstraintRow> Constraints { get; set; } = new();

        public class ConstraintRow
        {
            public int ID { get; set; } = 0;
            public string PTeamSize { get; set; } = string.Empty;
            public string SLoadCap { get; set; } = string.Empty;
            public string ALoadCap { get; set; } = string.Empty;
            public string PrefRankLimit { get; set; } = string.Empty;
        }
    }
}
