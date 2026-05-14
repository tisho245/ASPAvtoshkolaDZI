using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = RoleNames.CourseStudent)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(FilterModel? filter = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home", new { area = "" });

            var query = _context.StudentCourseInstances
                .Include(s => s.Instructor)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .Where(s => s.StudentId == userId)
                .AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(s => 
                        s.CourseInstances!.Courses!.Name.Contains(filter.SearchTerm) ||
                        s.Instructor!.FirstName.Contains(filter.SearchTerm) ||
                        s.Instructor.LastName.Contains(filter.SearchTerm));
                }

                if (!string.IsNullOrEmpty(filter.CategoryFilter))
                {
                    query = query.Where(s => s.CourseInstances!.Courses!.CategoryId.ToString() == filter.CategoryFilter);
                }

                if (!string.IsNullOrEmpty(filter.SortBy))
                {
                    query = filter.SortBy switch
                    {
                        "date" => filter.SortDescending ? query.OrderByDescending(s => s.CreateAt) : query.OrderBy(s => s.CreateAt),
                        "theory" => filter.SortDescending ? query.OrderByDescending(s => s.CurrentTheoryHours) : query.OrderBy(s => s.CurrentTheoryHours),
                        "practice" => filter.SortDescending ? query.OrderByDescending(s => s.CurrentPracticeHours) : query.OrderBy(s => s.CurrentPracticeHours),
                        _ => query.OrderByDescending(s => s.CreateAt)
                    };
                }
                else
                {
                    query = query.OrderByDescending(s => s.CreateAt);
                }
            }
            else
            {
                query = query.OrderByDescending(s => s.CreateAt);
            }

            var enrollments = await query.ToListAsync();

            // Данни за таблото на курсиста
            ViewBag.TotalEnrollments = enrollments.Count;
            ViewBag.TotalTheoryHours = enrollments.Sum(e => e.CurrentTheoryHours);
            ViewBag.TotalPracticeHours = enrollments.Sum(e => e.CurrentPracticeHours);
            ViewBag.Filter = filter ?? new FilterModel();

            return View(enrollments);
        }

        // Отделна страница "Моите записи" – списък с всички записвания
        public async Task<IActionResult> MyEnrollments(FilterModel? filter = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home", new { area = "" });

            var query = _context.StudentCourseInstances
                .Include(s => s.Instructor)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .Where(s => s.StudentId == userId)
                .AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(s => 
                        s.CourseInstances!.Courses!.Name.Contains(filter.SearchTerm) ||
                        s.Instructor!.FirstName.Contains(filter.SearchTerm) ||
                        s.Instructor.LastName.Contains(filter.SearchTerm));
                }

                if (!string.IsNullOrEmpty(filter.CategoryFilter))
                {
                    query = query.Where(s => s.CourseInstances!.Courses!.CategoryId.ToString() == filter.CategoryFilter);
                }

                if (!string.IsNullOrEmpty(filter.SortBy))
                {
                    query = filter.SortBy switch
                    {
                        "date" => filter.SortDescending ? query.OrderByDescending(s => s.CreateAt) : query.OrderBy(s => s.CreateAt),
                        "theory" => filter.SortDescending ? query.OrderByDescending(s => s.CurrentTheoryHours) : query.OrderBy(s => s.CurrentTheoryHours),
                        "practice" => filter.SortDescending ? query.OrderByDescending(s => s.CurrentPracticeHours) : query.OrderBy(s => s.CurrentPracticeHours),
                        _ => query.OrderByDescending(s => s.CreateAt)
                    };
                }
                else
                {
                    query = query.OrderByDescending(s => s.CreateAt);
                }
            }
            else
            {
                query = query.OrderByDescending(s => s.CreateAt);
            }

            var enrollments = await query.ToListAsync();
            ViewBag.Filter = filter ?? new FilterModel();

            return View(enrollments);
        }
    }
}
