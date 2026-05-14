using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers
{
    [Authorize(Roles = RoleNames.Administrator)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(FilterModel? filter = null)
        {
            ViewBag.StudentsCount = (await _userManager.GetUsersInRoleAsync(RoleNames.CourseStudent)).Count;
            ViewBag.InstructorsCount = (await _userManager.GetUsersInRoleAsync(RoleNames.Instructor)).Count;
            ViewBag.EnrollmentsCount = await _context.StudentCourseInstances.CountAsync();
            ViewBag.TotalHours = await _context.StudentCourseInstances.SumAsync(s => s.CurrentTheoryHours + s.CurrentPracticeHours);
            ViewBag.CategoriesCount = await _context.Categories.CountAsync();
            ViewBag.CourseInstancesCount = await _context.CourseInstances.CountAsync();
            
            var query = _context.StudentCourseInstances
                .Include(s => s.Student)
                .Include(s => s.Instructor)
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    query = query.Where(s => 
                        s.Student!.FirstName.Contains(filter.SearchTerm) ||
                        s.Student.LastName.Contains(filter.SearchTerm) ||
                        s.Instructor!.FirstName.Contains(filter.SearchTerm) ||
                        s.Instructor.LastName.Contains(filter.SearchTerm) ||
                        s.CourseInstances!.Courses!.Name.Contains(filter.SearchTerm));
                }

                if (!string.IsNullOrEmpty(filter.CategoryFilter))
                {
                    query = query.Where(s => s.CourseInstances!.Courses!.CategoryId.ToString() == filter.CategoryFilter);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(s => s.CreateAt >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(s => s.CreateAt <= filter.EndDate.Value);
                }

                if (!string.IsNullOrEmpty(filter.SortBy))
                {
                    query = filter.SortBy switch
                    {
                        "date" => filter.SortDescending ? query.OrderByDescending(s => s.CreateAt) : query.OrderBy(s => s.CreateAt),
                        "hours" => filter.SortDescending ? query.OrderByDescending(s => s.CurrentTheoryHours + s.CurrentPracticeHours) : query.OrderBy(s => s.CurrentTheoryHours + s.CurrentPracticeHours),
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

            var recent = await query.Take(10).ToListAsync();
            ViewBag.RecentEnrollments = recent;
            ViewBag.Filter = filter ?? new FilterModel();

            return View("~/Views/Dashboard/Index.cshtml");
        }
    }
}
