using System.Collections.Generic;

namespace FYP_25_S3_15P.ViewModels
{
    public record ScStudent(string Id, string Name, string Email);
    public record ScPerson(string Name, string Email);

    public class ScGroupDetailsVm
    {
        public string GroupId { get; set; } = "";
        public string ProjectTitle { get; set; } = "";
        public string[] Modules { get; set; } = System.Array.Empty<string>();
        public List<ScStudent> Students { get; set; } = new();
        public ScPerson Supervisor { get; set; } = new("", "");
        public ScPerson Assessor { get; set; } = new("", "");
    }
}
