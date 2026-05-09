using Avtoshkola_DZI.Data;
using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = RoleNames.Student)]
    public class EnrollmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public EnrollmentsController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> RequestCourse()
        {
            await FillDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCourse(int courseInstanceId, string instructorId, int vehicleId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home", new { area = "" });

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

        private async Task FillDropdownsAsync()
        {
            var instances = await _context.CourseInstances
                .Include(c => c.Courses)
                .OrderBy(c => c.StartDate)
                .ToListAsync();
            ViewBag.CourseInstanceId = new SelectList(
                instances.Select(i => new { i.Id, Name = $"{i.Courses?.Name} – {i.Description} ({i.StartDate:dd.MM.yy} – {i.EndDate:dd.MM.yy})" }),
                "Id", "Name");

            var instructors = await _userManager.GetUsersInRoleAsync(RoleNames.Instructor);
            ViewBag.InstructorId = new SelectList(
                instructors.Select(i => new { i.Id, Name = $"{i.FirstName} {i.LastName} ({i.Email})" }),
                "Id", "Name");

            var vehicles = await _context.Vehicles.Include(v => v.Categories).OrderBy(v => v.Brand).ToListAsync();
            ViewBag.VehicleId = new SelectList(
                vehicles.Select(v => new { v.Id, Name = $"{v.Brand} {v.Model} ({v.Categories?.Name})" }),
                "Id", "Name");
        }
    }
}
