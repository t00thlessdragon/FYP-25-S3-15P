using Microsoft.Identity.Client;

namespace FYP.Models
{
    public class Supervisor
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public List<ProjectGroup> Groups { get; set; }
    }

    public class ProjectGroup
    {
        public string Code { get; set; }
        public string Title { get; set; }
    }
}