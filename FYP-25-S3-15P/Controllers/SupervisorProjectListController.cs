using FYP.Models;
using Microsoft.AspNetCore.Mvc;

namespace FYP.Controllers
{
    public class SupervisorProjectListController : Controller
    {
        public IActionResult ViewProject()
        {
            var projects = new List<SupervisorProjectListing>
            {
                new SupervisorProjectListing
                {
                    Id = 1,
                    ProjectID = "CSIT-25-S3-08",
                    ProjectTitle = "A Smart Project Allocation System",
                    Description = "A system to solve multi-constraint optimisation for project allocation, ensuring fair distribution among students.",
                    Tags = new List<string> { "Allocation", "System" }
                },

                new SupervisorProjectListing
                {
                    Id = 2,
                    ProjectID = "CSIT-25-S3-09",
                    ProjectTitle = "Identifying cryptographic functions",
                    Description = "Blockchain systems are using various cryptographic functions. Identifying which cryptographic libraries and they are using or referencing is useful to enhance their security.",
                    Tags = new List<string> { "Blockchain", "Security" }
                }
            };

            var viewModel = new ProjectListingView
            {
                Projects = projects
            };

            return View("SupervisorProjectList", viewModel);
        }

        // Action for the "View Details" button
        public IActionResult ViewDetails(int id)
        {
            // Placeholder for future logic (e.g., show a new page with project details)
            TempData["Message"] = $"View Details button clicked for Project ID: {id}";
            return RedirectToAction("ManageProject");
        }

        // Action for the "Add to Preferences" button
        public IActionResult AddToPreferences(int id)
        {
            // Placeholder for future logic (e.g., add project to a list)
            TempData["Message"] = $"Add to Preferences button clicked for Project ID: {id}";
            return RedirectToAction("ManageProject");
        }
    }
}