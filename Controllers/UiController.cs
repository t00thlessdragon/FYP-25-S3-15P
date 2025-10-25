using Microsoft.AspNetCore.Mvc;
using FYP_25_S3_15P.Models;

namespace FYP_25_S3_15P.Controllers
{
    public class UiController : Controller
    {
        // Preview route: /preview/login
        [HttpGet("/preview/login")]
        public IActionResult LoginPreview()
            => View("~/Views/Account/Login.cshtml", new Login());
    }
}