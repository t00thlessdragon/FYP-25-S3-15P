using FYP.Models;
using Microsoft.AspNetCore.Mvc;

namespace FYP.Controllers
{
    // Supervisor/...
    public class SupervisorDashboardController : Controller
    {
        // Dashboard
        public ActionResult Dashboard()
        {
            var supervisor = GetSupervisor();
            return View(supervisor);
        }

        // View Profile
        public ActionResult ViewProfile()
        {
            var supervisor = GetSupervisor();
            return View(supervisor);
        }

        // Hardcoded Supervisor data
        private Supervisor GetSupervisor()
        {
            return new Supervisor
            {
                Groups = new List<ProjectGroup>
                {
                    new ProjectGroup
                    {
                        Code = "FYP-25-S3-14P",
                        Title = "Data Warehouse Refresh"
                    },

                    new ProjectGroup
                    {
                        Code = "FYP-25-S3-15P",
                        Title = "Smart Project Allocation System"
                    }
                }
            };
        }
    }
}