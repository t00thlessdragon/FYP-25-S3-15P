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
                Name = "John Doe",
                Role = "Supervisor",
                SupervisorID = "223456",
                Email = "john_doe@gmail.com",
                Phone = "+6512345687",
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
                Name = "John Doe",
                Role = "Supervisor",
                SupervisorID = "223456",
                Email = "john_doe@gmail.com",
                Phone = "+6512345687",
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
                Name = "John Doe",
                Role = "Supervisor",
                SupervisorID = "223456",
                Email = "john_doe@gmail.com",
                Phone = "+6512345687",
                Department = "Information Technology",
                Position = "Senior Lecturer",
                AreaOfExpertise = "Data Analytics, Project Management",
                DateJoined = "15 Jan 2021"
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


    
    
