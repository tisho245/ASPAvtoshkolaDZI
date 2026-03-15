using System.Diagnostics;
using Avtoshkola_DZI.Data;
using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<Client> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _userManager.GetUsersInRoleAsync(RoleNames.Student);
            var instructors = await _userManager.GetUsersInRoleAsync(RoleNames.Instructor);
            var enrollmentsCount = await _context.StudentCourseInstances.CountAsync();
            var totalHours = await _context.StudentCourseInstances
                .SumAsync(s => s.CurrentTheoryHours + s.CurrentPracticeHours);
            var categoriesCount = await _context.Categories.CountAsync();

            ViewBag.StudentsCount = students.Count;
            ViewBag.InstructorsCount = instructors.Count;
            ViewBag.EnrollmentsCount = enrollmentsCount;
            ViewBag.TotalHours = totalHours;
            ViewBag.CategoriesCount = categoriesCount;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> About()
        {
            var students = await _userManager.GetUsersInRoleAsync(RoleNames.Student);
            var instructors = await _userManager.GetUsersInRoleAsync(RoleNames.Instructor);
            var categoriesCount = await _context.Categories.CountAsync();
            ViewBag.StudentsCount = students.Count;
            ViewBag.InstructorsCount = instructors.Count;
            ViewBag.CategoriesCount = categoriesCount;
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public async Task<IActionResult> Services()
        {
            var courses = await _context.Courses
                .Include(c => c.Categories)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(courses);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
