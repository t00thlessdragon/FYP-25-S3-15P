using Microsoft.AspNetCore.Mvc;

namespace FYP_25_S3_15P.Controllers
{
    public class UserManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
