using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers
{
    [Authorize]
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public EnrollmentsController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Admin: Full CRUD access to all enrollments
        [Authorize(Roles = RoleNames.Administrator)]
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

        [Authorize(Roles = RoleNames.Administrator)]
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

        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Create()
        {
            await FillDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.Administrator)]
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

        [Authorize(Roles = RoleNames.Administrator)]
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
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Edit(int id, int courseInstanceId, string studentId, string instructorId, int vehicleId, DateTime createAt, int currentTheoryHours, int currentPracticeHours)
        {
            var item = await _context.StudentCourseInstances
                .Include(s => s.CourseInstances)
                    .ThenInclude(c => c.Courses)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (item == null) return NotFound();

            // Валидация за максимални часове
            var maxTheoryHours = item.CourseInstances?.Courses?.TotalTheoryHours ?? 0;
            var maxPracticeHours = item.CourseInstances?.Courses?.TotalPracticeHours ?? 0;

            if (currentTheoryHours > maxTheoryHours)
            {
                ModelState.AddModelError("", $"Часовете теория ({currentTheoryHours}) не могат да надвишават максималните за курса ({maxTheoryHours}).");
                await FillDropdownsAsync();
                return View(item);
            }

            if (currentPracticeHours > maxPracticeHours)
            {
                ModelState.AddModelError("", $"Часовете практика ({currentPracticeHours}) не могат да надвишават максималните за курса ({maxPracticeHours}).");
                await FillDropdownsAsync();
                return View(item);
            }

            if (currentTheoryHours < 0 || currentPracticeHours < 0)
            {
                ModelState.AddModelError("", "Часовете не могат да бъдат отрицателни числа.");
                await FillDropdownsAsync();
                return View(item);
            }

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

        [Authorize(Roles = RoleNames.Administrator)]
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
        [Authorize(Roles = RoleNames.Administrator)]
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

        // Student: Request course enrollment
        [Authorize(Roles = RoleNames.CourseStudent)]
        public async Task<IActionResult> Request()
        {
            await FillStudentDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.CourseStudent)]
        public async Task<IActionResult> Request(int courseInstanceId, string instructorId, int vehicleId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home");

            // Валидация за съвпадение на категорията между курса и МПС
            var courseInstance = await _context.CourseInstances
                .Include(c => c.Courses)
                .ThenInclude(c => c.Categories)
                .FirstOrDefaultAsync(c => c.Id == courseInstanceId);
            
            var vehicle = await _context.Vehicles
                .Include(v => v.Categories)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (courseInstance?.Courses?.CategoryId != vehicle?.CategoryId)
            {
                ModelState.AddModelError("", $"МПС '{vehicle?.Brand} {vehicle?.Model}' е от категория '{vehicle?.Categories?.Name}', но избраният курс е от категория '{courseInstance?.Courses?.Categories?.Name}'. Моля, изберете МПС от същата категория като курса.");
                await FillStudentDropdownsAsync();
                return View();
            }

            var enrollment = new StudentCourseInstance
            {
                CourseInstanceId = courseInstanceId,
                StudentId = userId,
                InstructorId = instructorId,
                VehicleId = vehicleId,
                CreateAt = DateTime.UtcNow,
                CurrentTheoryHours = 0,
                CurrentPracticeHours = 0
            };

            _context.StudentCourseInstances.Add(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home", new { area = "Student" });
        }

        private async Task FillDropdownsAsync(StudentCourseInstance? current = null)
        {
            var instances = await _context.CourseInstances.Include(c => c.Courses).OrderBy(c => c.StartDate).ToListAsync();
            ViewBag.CourseInstanceId = new SelectList(instances.Select(i => new { i.Id, Name = $"{i.Courses?.Name} – {i.Description} (ID: {i.Id})" }), "Id", "Name", current?.CourseInstanceId);

            var students = await _userManager.GetUsersInRoleAsync(RoleNames.CourseStudent);
            ViewBag.StudentId = new SelectList(students.Select(k => new { k.Id, Name = $"{k.FirstName} {k.LastName} ({k.Email})" }), "Id", "Name", current?.StudentId);

            var instructors = await _userManager.GetUsersInRoleAsync(RoleNames.Instructor);
            ViewBag.InstructorId = new SelectList(instructors.Select(i => new { i.Id, Name = $"{i.FirstName} {i.LastName} ({i.Email})" }), "Id", "Name", current?.InstructorId);

            var vehicles = await _context.Vehicles.Include(v => v.Categories).OrderBy(v => v.Brand).ToListAsync();
            ViewBag.VehicleId = new SelectList(vehicles.Select(v => new { v.Id, Name = $"{v.Brand} {v.Model} (ID: {v.Id})" }), "Id", "Name", current?.VehicleId);
        }

        private async Task FillStudentDropdownsAsync()
        {
            var instances = await _context.CourseInstances
                .Include(c => c.Courses)
                .ThenInclude(c => c.Categories)
                .OrderBy(c => c.StartDate)
                .ToListAsync();
            ViewBag.CourseInstanceId = new SelectList(
                instances.Select(i => new { 
                    i.Id, 
                    Name = $"{i.Courses?.Name} – {i.Description} ({i.StartDate:dd.MM.yy} – {i.EndDate:dd.MM.yy})"
                }),
                "Id", "Name");
                
            ViewBag.CoursesWithCategories = instances.Select(i => new { 
                i.Id, 
                Name = $"{i.Courses?.Name} – {i.Description} ({i.StartDate:dd.MM.yy} – {i.EndDate:dd.MM.yy})",
                CategoryId = i.Courses?.CategoryId.ToString() ?? ""
            }).ToList();

            var instructors = await _userManager.GetUsersInRoleAsync(RoleNames.Instructor);
            ViewBag.InstructorId = new SelectList(
                instructors.Select(i => new { i.Id, Name = $"{i.FirstName} {i.LastName} ({i.Email})" }),
                "Id", "Name");

            var vehicles = await _context.Vehicles.Include(v => v.Categories).OrderBy(v => v.Brand).ToListAsync();
            ViewBag.AllVehicles = vehicles.Select(v => new { 
                v.Id, 
                Name = $"{v.Brand} {v.Model} ({v.Categories?.Name})",
                CategoryId = v.CategoryId
            }).ToList();
            
            ViewBag.VehicleId = new SelectList(
                ViewBag.AllVehicles,
                "Id", "Name");
        }
    }
}
