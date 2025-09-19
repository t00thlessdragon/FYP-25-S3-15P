using Microsoft.Identity.Client;

namespace FYP.Models
{
    public class SupervisorAllocatedGroup
    {
        public string GroupName { get; set; }
        public string ProjectID { get; set; }
        public string ProjectTitle { get; set; }
    }

    public class AllocatedGroupView
    {
        public List<SupervisorAllocatedGroup> AllocatedGroups { get; set; }
    }

}