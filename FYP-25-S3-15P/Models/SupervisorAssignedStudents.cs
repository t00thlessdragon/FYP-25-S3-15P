using Microsoft.Identity.Client;

namespace FYP.Models
{
    public class SupervisorAssignedStudents
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string StudentName { get; set; }
        public string ProjectTitle { get; set; }
    }

    public class AssignedStudentsList
    {
        public List<SupervisorAssignedStudents> Students { get; set; }
    }
}