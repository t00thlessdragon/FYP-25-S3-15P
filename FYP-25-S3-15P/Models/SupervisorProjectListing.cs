namespace FYP.Models
{
    public class SupervisorProjectListing
    {
        public int Id { get; set; }
        public string ProjectID { get; set; }
        public string ProjectTitle { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
    }

    public class ProjectListingView
    {
        public List<SupervisorProjectListing> Projects { get; set; }
    }

}