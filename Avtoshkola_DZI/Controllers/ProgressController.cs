using Avtoshkola_DZI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers
{
    [Authorize(Roles = RoleNames.Student)]
    public class ProgressController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public ProgressController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>Моят прогрес – записи (StudentCourseInstance) за текущия потребител с часове от таблиците.</summary>
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return View(new List<StudentCourseInstance>());

            var list = await _context.StudentCourseInstances
                .Where(s => s.StudentId == userId)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Instructor)
                .Include(s => s.Vehicles)
                .OrderByDescending(s => s.CreateAt)
                .ToListAsync();

            return View(list);
        }
    }
}
