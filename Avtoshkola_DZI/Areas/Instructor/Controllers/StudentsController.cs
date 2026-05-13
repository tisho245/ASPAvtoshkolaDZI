using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Areas.Instructor.Controllers
{
    [Authorize(Roles = RoleNames.Instructor)]
    [Area("Instructor")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public StudentsController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Instructor/Students
        public async Task<IActionResult> Index()
        {
            var instructorId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(instructorId))
            {
                return View(new List<Client>());
            }

            // Вземи всички курсисти, които имат записи за курсове с този инструктор
            var students = await _context.StudentCourseInstances
                .Where(sci => sci.InstructorId == instructorId)
                .Select(sci => sci.Student)
                .Distinct()
                .ToListAsync();

            return View(students);
        }



        // GET: Instructor/Students/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructorId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(instructorId))
            {
                return NotFound();
            }

            // Провери дали този курсист е на текущия инструктор
            var isInstructorStudent = await _context.StudentCourseInstances
                .AnyAsync(sci => sci.StudentId == id && sci.InstructorId == instructorId);

            if (!isInstructorStudent)
            {
                return Forbid();
            }

            var student = await _context.Users
                .OfType<Client>()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null)
            {
                return NotFound();
            }



            // Вземи прогреса на курсиста
            var enrollments = await _context.StudentCourseInstances
                .Include(sci => sci.CourseInstances)
                    .ThenInclude(ci => ci.Courses)
                .Include(sci => sci.Vehicles)
                .Where(sci => sci.StudentId == id && sci.InstructorId == instructorId)
                .ToListAsync();

            ViewBag.Enrollments = enrollments;



            return View(student);
        }
    }
}
