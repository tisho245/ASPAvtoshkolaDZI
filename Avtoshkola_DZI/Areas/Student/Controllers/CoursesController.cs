using Avtoshkola_DZI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = RoleNames.Student)]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Извличане на информация за обучение – налични курсове и предстоящи издания
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Categories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(courses);
        }
    }
}

