using FYP.Models;
using Microsoft.AspNetCore.Mvc;

namespace FYP.Controllers
{
    public class SupervisorAssignedStudentsController : Controller
    {
        public IActionResult ViewStudents()
        {
            var students = new List<SupervisorAssignedStudents>
            {
                new SupervisorAssignedStudents { Id = 1, GroupName = "FYP-25-S3-14P", StudentName = "Andy", ProjectTitle = "Data Warehouse Refresh" },
                new SupervisorAssignedStudents { Id = 2, GroupName = "FYP-25-S3-14P", StudentName = "Baccy", ProjectTitle = "Data Warehouse Refresh" },
                new SupervisorAssignedStudents { Id = 3, GroupName = "FYP-25-S3-14P", StudentName = "Carrie", ProjectTitle = "Data Warehouse Refresh" },
                new SupervisorAssignedStudents { Id = 4, GroupName = "FYP-25-S3-14P", StudentName = "Jessie", ProjectTitle = "Data Warehouse Refresh" },
                new SupervisorAssignedStudents { Id = 5, GroupName = "FYP-25-S3-14P", StudentName = "Dally", ProjectTitle = "Data Warehouse Refresh" },
                new SupervisorAssignedStudents { Id = 6, GroupName = "FYP-25-S3-15P", StudentName = "Sam", ProjectTitle = "Smart Project Allocation System" },
                new SupervisorAssignedStudents { Id = 7, GroupName = "FYP-25-S3-15P", StudentName = "Abigail", ProjectTitle = "Smart Project Allocation System" },
                new SupervisorAssignedStudents { Id = 8, GroupName = "FYP-25-S3-15P", StudentName = "Benjamin", ProjectTitle = "Smart Project Allocation System" },
                new SupervisorAssignedStudents { Id = 9, GroupName = "FYP-25-S3-15P", StudentName = "Justin", ProjectTitle = "Smart Project Allocation System" },
                new SupervisorAssignedStudents { Id = 10, GroupName = "FYP-25-S3-15P", StudentName = "Isaac", ProjectTitle = "Smart Project Allocation System" }
            };

            var viewModel = new AssignedStudentsList { Students = students };

            return View("SupervisorAssignedStudentList", viewModel);
        }
    }
}