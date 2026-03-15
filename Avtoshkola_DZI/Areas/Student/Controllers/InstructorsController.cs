using Avtoshkola_DZI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = RoleNames.Student)]
    public class InstructorsController : Controller
    {
        private readonly UserManager<Client> _userManager;
        private readonly ApplicationDbContext _context;

        public InstructorsController(UserManager<Client> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // READ – списък с инструктори за курсисти
        public async Task<IActionResult> Index()
        {
            var instructors = await _userManager.GetUsersInRoleAsync(RoleNames.Instructor);
            return View(instructors);
        }

        // READ – подробности за конкретен инструктор
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();

            var instructor = await _userManager.FindByIdAsync(id);
            if (instructor == null || !await _userManager.IsInRoleAsync(instructor, RoleNames.Instructor))
                return NotFound();

            // допълнителна информация – активни записи по курсове с този инструктор
            var activeCourses = await _context.StudentCourseInstances
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .Where(s => s.InstructorId == id)
                .OrderByDescending(s => s.CreateAt)
                .ToListAsync();

            ViewBag.ActiveCourses = activeCourses;
            return View(instructor);
        }
    }
}

