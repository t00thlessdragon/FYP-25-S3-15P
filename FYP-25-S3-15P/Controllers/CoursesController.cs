using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FYP_25_S3_15P.Data;
using FYP_25_S3_15P.Models;
using FYP_25_S3_15P.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FYP_25_S3_15P.Controllers
{
    public class CoursesController : Controller
    {
        private readonly SmartDbContext _context;

        public CoursesController(SmartDbContext context)
        {
            _context = context;
        }

        // Display all course listing
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses.ToListAsync();
            return View(courses);
        }

        // Display individual course
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.ID == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
    }
}
