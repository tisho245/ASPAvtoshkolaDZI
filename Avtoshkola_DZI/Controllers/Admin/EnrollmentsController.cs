using Avtoshkola_DZI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers.Admin
{
    [Authorize(Roles = RoleNames.Administrator)]
    [Area("Admin")]
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public EnrollmentsController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.StudentCourseInstances
                .Include(s => s.Student)
                .Include(s => s.Instructor)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .OrderByDescending(s => s.CreateAt)
                .ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.StudentCourseInstances
                .Include(s => s.Student)
                .Include(s => s.Instructor)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        public async Task<IActionResult> Create()
        {
            await FillDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int courseInstanceId, string studentId, string instructorId, int vehicleId)
        {
            var enrollment = new StudentCourseInstance
            {
                CourseInstanceId = courseInstanceId,
                StudentId = studentId,
                InstructorId = instructorId,
                VehicleId = vehicleId,
                CreateAt = DateTime.UtcNow,
                CurrentTheoryHours = 0,
                CurrentPracticeHours = 0
            };
            _context.StudentCourseInstances.Add(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.StudentCourseInstances.FindAsync(id);
            if (item == null) return NotFound();
            await FillDropdownsAsync(item);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int courseInstanceId, string studentId, string instructorId, int vehicleId, DateTime createAt, int currentTheoryHours, int currentPracticeHours)
        {
            var item = await _context.StudentCourseInstances.FindAsync(id);
            if (item == null) return NotFound();
            item.CourseInstanceId = courseInstanceId;
            item.StudentId = studentId;
            item.InstructorId = instructorId;
            item.VehicleId = vehicleId;
            item.CreateAt = createAt;
            item.CurrentTheoryHours = currentTheoryHours;
            item.CurrentPracticeHours = currentPracticeHours;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.StudentCourseInstances
                .Include(s => s.Student)
                .Include(s => s.Instructor)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.StudentCourseInstances.FindAsync(id);
            if (item != null)
            {
                _context.StudentCourseInstances.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task FillDropdownsAsync(StudentCourseInstance? current = null)
        {
            var instances = await _context.CourseInstances.Include(c => c.Courses).OrderBy(c => c.StartDate).ToListAsync();
            ViewBag.CourseInstanceId = new SelectList(instances.Select(i => new { i.Id, Name = $"{i.Courses?.Name} – {i.Description} (ID: {i.Id})" }), "Id", "Name", current?.CourseInstanceId);

            var students = await _userManager.GetUsersInRoleAsync(RoleNames.Student);
            ViewBag.StudentId = new SelectList(students.Select(k => new { k.Id, Name = $"{k.FirstName} {k.LastName} ({k.Email})" }), "Id", "Name", current?.StudentId);

            var instructors = await _userManager.GetUsersInRoleAsync(RoleNames.Instructor);
            ViewBag.InstructorId = new SelectList(instructors.Select(i => new { i.Id, Name = $"{i.FirstName} {i.LastName} ({i.Email})" }), "Id", "Name", current?.InstructorId);

            var vehicles = await _context.Vehicles.Include(v => v.Categories).OrderBy(v => v.Brand).ToListAsync();
            ViewBag.VehicleId = new SelectList(vehicles.Select(v => new { v.Id, Name = $"{v.Brand} {v.Model} (ID: {v.Id})" }), "Id", "Name", current?.VehicleId);
        }
    }
}
