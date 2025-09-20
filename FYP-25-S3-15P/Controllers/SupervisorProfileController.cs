using FYP.Models;
using Microsoft.AspNetCore.Mvc;

namespace FYP.Controllers
{
    public class SupervisorProfileController : Controller
    {
        public IActionResult ViewProfile()
        {
            var profile = new SupervisorProfile
            {
                Name = "Peter Tan",
                Role = "Supervisor",
                SupervisorID = "Sup12345",
                Email = "peter_tan@university_domain.com",
                Phone = "+6591234568",
                Department = "Information Technology",
                Position = "Senior Lecturer",
                DateJoined = "15 Jan 2021",
                AreaOfExpertise = "Data Analytics, Project Management"
            };
            return View("SupervisorProfilePage", profile);
        }
        
        public IActionResult AddUpdateExpertise()
        {
            var profile = new SupervisorProfile
            {
                Name = "Peter Tan",
                Role = "Supervisor",
                SupervisorID = "Sup12345",
                Email = "peter_tan@university_domain.com",
                Phone = "+6591234568",
                Department = "Information Technology",
                Position = "Senior Lecturer",
                DateJoined = "15 Jan 2021",
                AreaOfExpertise = "Data Analytics, Project Management"
            };
            return View("SupervisorEditExpertise", profile);
        }
        [HttpPost]
        public IActionResult SaveExpertise(SupervisorProfile model)
        {
            TempData["Message"] = "Area of Expertise updated successfully!";
            return RedirectToAction("ViewProfile"); // Redirect to the read-only profile view
        }

        public IActionResult EditProfile()
        {
            var profile = new SupervisorProfile
            {
                Name = "Peter Tan",
                Role = "Supervisor",
                SupervisorID = "Sup12345",
                Email = "peter_tan@university_domain.com",
                Phone = "+6591234568",
                Department = "Information Technology",
                Position = "Senior Lecturer",
                DateJoined = "15 Jan 2021",
                AreaOfExpertise = "Data Analytics, Project Management"
            };
            return View("SupervisorProfilePageEdit", profile);
        }

        [HttpPost]
        public IActionResult SaveProfile(SupervisorProfile model)
        {
            TempData["Message"] = "Saved successfully";
            return RedirectToAction("ViewProfile");

        }
    }
}


    
    
