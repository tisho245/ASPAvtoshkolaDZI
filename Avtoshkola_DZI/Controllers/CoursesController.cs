using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public CoursesController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Admin: Full CRUD access
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Index()
        {
            var list = await _context.Courses.Include(c => c.Categories).ToListAsync();
            return View(list);
        }

        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var course = await _context.Courses.Include(c => c.Categories).FirstOrDefaultAsync(m => m.Id == id);
            if (course == null) return NotFound();
            return View(course);
        }

        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Location,TotalTheoryHours,TotalPracticeHours,Price,CategoryId,CreatedAt")] Course course)
        {
            course.CreatedAt = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", course.CategoryId);
            return View(course);
        }

        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", course.CategoryId);
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Location,TotalTheoryHours,TotalPracticeHours,Price,CategoryId,CreatedAt")] Course course)
        {
            if (id != course.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Courses.AnyAsync(e => e.Id == course.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", course.CategoryId);
            return View(course);
        }

        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var course = await _context.Courses.Include(c => c.Categories).FirstOrDefaultAsync(m => m.Id == id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Student: Read-only access to available courses
        [Authorize(Roles = RoleNames.CourseStudent)]
        public async Task<IActionResult> Available()
        {
            var courses = await _context.Courses
                .Include(c => c.Categories)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(courses);
        }
    }
}
