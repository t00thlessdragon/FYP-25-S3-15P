namespace FYP_25_S3_15P.ViewModels
{
    public class AssessorProfileVm
    {
        // Header
        public string AvatarUrl { get; set; } = "/images/avatar-demo.png";
        public string Name { get; set; } = "Demo Assessor";
        public string Role { get; set; } = "Assessor";

        // Assessor details (editable)
        public string AssessorId { get; set; } = "123456";
        public string? Email { get; set; }
        public string? Phone { get; set; }

        // Professional details (read-only + tags)
        public string Program { get; set; } = "Information Technology";
        public string Title { get; set; } = "Assessor";

        // “Areas of Expertise” chips
        public List<string> Expertise { get; set; } = new() { "Artificial Intelligence", "Data Mining" };

        // helper to round-trip chips from hidden field
        public string? ExpertiseCsv
        {
            get => Expertise == null ? "" : string.Join(",", Expertise);
            set => Expertise = string.IsNullOrWhiteSpace(value)
                ? new List<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
    }
}
