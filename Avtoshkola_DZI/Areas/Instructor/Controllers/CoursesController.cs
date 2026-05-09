using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = RoleNames.Instructor)]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public CoursesController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetInstructorId() => _userManager.GetUserId(User);

        // Списък с всички мои записи (курсове) – входна точка за CRUD
        public async Task<IActionResult> Index()
        {
            var instructorId = GetInstructorId();
            if (string.IsNullOrEmpty(instructorId))
                return RedirectToAction("Index", "Home", new { area = "" });

            var items = await _context.StudentCourseInstances
                .Include(s => s.Student)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .Where(s => s.InstructorId == instructorId)
                .OrderByDescending(s => s.CreateAt)
                .ToListAsync();

            return View(items);
        }

        // Детайли за запис (курс на инструктор)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var instructorId = GetInstructorId();
            var item = await _context.StudentCourseInstances
                .Include(s => s.Student)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.Id == id && s.InstructorId == instructorId);
            if (item == null) return NotFound();
            return View(item);
        }

        // Редакция на часове по курс
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var instructorId = GetInstructorId();
            var item = await _context.StudentCourseInstances
                .Include(s => s.Student)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.Id == id && s.InstructorId == instructorId);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int currentTheoryHours, int currentPracticeHours)
        {
            var instructorId = GetInstructorId();
            var item = await _context.StudentCourseInstances
                .FirstOrDefaultAsync(s => s.Id == id && s.InstructorId == instructorId);
            if (item == null) return NotFound();

            item.CurrentTheoryHours = currentTheoryHours;
            item.CurrentPracticeHours = currentPracticeHours;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home", new { area = "Instructor" });
        }

        // Изтриване на запис (курс на инструктор)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var instructorId = GetInstructorId();
            var item = await _context.StudentCourseInstances
                .Include(s => s.Student)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(s => s.Id == id && s.InstructorId == instructorId);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructorId = GetInstructorId();
            var item = await _context.StudentCourseInstances
                .FirstOrDefaultAsync(s => s.Id == id && s.InstructorId == instructorId);
            if (item != null)
            {
                _context.StudentCourseInstances.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home", new { area = "Instructor" });
        }
    }
}

