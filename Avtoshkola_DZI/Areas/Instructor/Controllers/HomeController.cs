using Avtoshkola_DZI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = RoleNames.Instructor)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home", new { area = "" });

            var enrollments = await _context.StudentCourseInstances
                .Include(s => s.Student)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .Where(s => s.InstructorId == userId)
                .OrderByDescending(s => s.CreateAt)
                .ToListAsync();

            // Данни за таблото на инструктора
            ViewBag.TotalEnrollments = enrollments.Count;
            ViewBag.TotalStudents = enrollments.Select(e => e.StudentId).Distinct().Count();
            ViewBag.TotalTheoryHours = enrollments.Sum(e => e.CurrentTheoryHours);
            ViewBag.TotalPracticeHours = enrollments.Sum(e => e.CurrentPracticeHours);

            return View(enrollments);
        }
    }
}
