using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers.Admin
{
    [Authorize(Roles = RoleNames.Administrator)]
    [Area("Admin")]
    public class CourseInstancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CourseInstancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.CourseInstances.Include(c => c.Courses).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var instance = await _context.CourseInstances.Include(c => c.Courses).FirstOrDefaultAsync(m => m.Id == id);
            if (instance == null) return NotFound();
            return View(instance);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["CourseId"] = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseId,StartDate,EndDate,Description")] CourseInstance instance)
        {
            if (instance.StartDate > instance.EndDate)
            {
                ModelState.AddModelError(string.Empty, "Началната дата на курса не може да е след крайната дата.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(instance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", instance.CourseId);
            return View(instance);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var instance = await _context.CourseInstances.FindAsync(id);
            if (instance == null) return NotFound();
            ViewData["CourseId"] = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", instance.CourseId);
            return View(instance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,StartDate,EndDate,Description")] CourseInstance instance)
        {
            if (id != instance.Id) return NotFound();
            if (instance.StartDate > instance.EndDate)
            {
                ModelState.AddModelError(string.Empty, "Началната дата на курса не може да е след крайната дата.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(instance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.CourseInstances.AnyAsync(e => e.Id == instance.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", instance.CourseId);
            return View(instance);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var instance = await _context.CourseInstances.Include(c => c.Courses).FirstOrDefaultAsync(m => m.Id == id);
            if (instance == null) return NotFound();
            return View(instance);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instance = await _context.CourseInstances.FindAsync(id);
            if (instance != null)
            {
                _context.CourseInstances.Remove(instance);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
