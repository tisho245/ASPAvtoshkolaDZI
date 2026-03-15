using Avtoshkola_DZI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return View();

            // Инструкторите ползват своето табло в Area("Instructor")
            if (User.IsInRole(RoleNames.Instructor))
            {
                return RedirectToAction("Index", "Home", new { area = "Instructor" });
            }

            if (User.IsInRole(RoleNames.Administrator))
            {
                ViewBag.StudentsCount = (await _userManager.GetUsersInRoleAsync(RoleNames.Student)).Count;
                ViewBag.InstructorsCount = (await _userManager.GetUsersInRoleAsync(RoleNames.Instructor)).Count;
                ViewBag.EnrollmentsCount = await _context.StudentCourseInstances.CountAsync();
                ViewBag.TotalHours = await _context.StudentCourseInstances.SumAsync(s => s.CurrentTheoryHours + s.CurrentPracticeHours);
                ViewBag.CategoriesCount = await _context.Categories.CountAsync();
                ViewBag.CourseInstancesCount = await _context.CourseInstances.CountAsync();
                var recent = await _context.StudentCourseInstances
                    .OrderByDescending(s => s.CreateAt)
                    .Take(10)
                    .Include(s => s.Student)
                    .Include(s => s.Instructor)
                    .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                    .ToListAsync();
                ViewBag.RecentEnrollments = recent;
            }
            else if (User.IsInRole(RoleNames.Student))
            {
                ViewBag.MyEnrollmentsCount = await _context.StudentCourseInstances.CountAsync(s => s.StudentId == userId);
                var recent = await _context.StudentCourseInstances
                    .Where(s => s.StudentId == userId)
                    .OrderByDescending(s => s.CreateAt)
                    .Take(10)
                    .Include(s => s.Instructor)
                    .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                    .ToListAsync();
                ViewBag.RecentEnrollments = recent;
            }

            return View();
        }
    }
}
