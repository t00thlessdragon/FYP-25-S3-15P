using FYP.Models;
using Microsoft.AspNetCore.Mvc;

namespace FYP.Controllers
{
    public class SupervisorAllocatedGroupController : Controller
    {
        public IActionResult ViewAllocatedGroup()
        {
            var groups = new List<SupervisorAllocatedGroup>
            {
                new SupervisorAllocatedGroup
                {
                    GroupName = "FYP-25-S3-14P",
                    ProjectID = "CSIT-25-S3-08",
                    ProjectTitle = "Data Warehouse Refresh"
                },
                new SupervisorAllocatedGroup
                {
                    GroupName = "FYP-25-S3-15P",
                    ProjectID = "CSIT-25-S3-09",
                    ProjectTitle = "Smart Project Allocation System"
                }
            };
            // For testing the "No groups assigned" scenario, use this instead:
            // var groups = new List<GroupModel>();

            var viewModel = new AllocatedGroupView { AllocatedGroups = groups };

            return View("SupervisorAllocatedGroupList", viewModel);


        }
        // Placeholder actions for each button.
        public IActionResult AssignTasks(string id)
        {
            TempData["Message"] = $"Assign Tasks button clicked for group: {id}";
            return RedirectToAction("ViewAllocatedGroup");
        }

        public IActionResult ViewTasks(string id)
        {
            TempData["Message"] = $"View Tasks button clicked for group: {id}";
            return RedirectToAction("ViewAllocatedGroup");
        }
        
        public IActionResult ScheduleMeeting(string id)
        {
            TempData["Message"] = $"Schedule Meeting button clicked for group: {id}";
            return RedirectToAction("ViewAllocatedGroup");
        }
        
        public IActionResult SendAnnouncement(string id)
        {
            TempData["Message"] = $"Send Announcement button clicked for group: {id}";
            return RedirectToAction("ViewAllocatedGroup");
        }
        
        public IActionResult TrackProgress(string id)
        {
            TempData["Message"] = $"Track Progress button clicked for group: {id}";
            return RedirectToAction("ViewAllocatedGroup");
        }
        
        public IActionResult SubmitEvaluation(string id)
        {
            TempData["Message"] = $"Submit Evaluation button clicked for group: {id}";
            return RedirectToAction("ViewAllocatedGroup");
        }
    }
}