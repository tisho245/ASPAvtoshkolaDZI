using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers
{
    [Authorize]
    public class InstructorsController : Controller
    {
        private readonly UserManager<Client> _userManager;
        private readonly ApplicationDbContext _context;

        public InstructorsController(UserManager<Client> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Admin: Full CRUD access to all instructors
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Index()
        {
            var instructors = await _userManager.GetUsersInRoleAsync(RoleNames.Instructor);
            return View(instructors);
        }

        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();
            var instructor = await _userManager.FindByIdAsync(id);
            if (instructor == null || !await _userManager.IsInRoleAsync(instructor, RoleNames.Instructor))
                return NotFound();
            return View(instructor);
        }

        [Authorize(Roles = RoleNames.Administrator)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Create([Bind("Id,UserName,FirstName,LastName,Email,PhoneNumber,Description,CreatedAt")] Client client, [FromForm(Name = "Photo")] IFormFile? photoFile, string password)
        {
            client.EmailConfirmed = true;
            client.Description ??= "";
            var file = photoFile ?? Request.Form.Files["Photo"];
            var photoBytes = await GetPhotoBytesAsync(file);
            client.PhotoData = photoBytes;
            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(client, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(client, RoleNames.Instructor);
                    if (photoBytes != null && photoBytes.Length > 0)
                    {
                        var dbUser = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == client.Id);
                        if (dbUser != null)
                        {
                            dbUser.PhotoData = photoBytes;
                            await _context.SaveChangesAsync();
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
            }
            return View(client);
        }

        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();
            var instructor = await _userManager.FindByIdAsync(id);
            if (instructor == null || !await _userManager.IsInRoleAsync(instructor, RoleNames.Instructor))
                return NotFound();
            return View(instructor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,FirstName,LastName,Email,PhoneNumber,Description,CreatedAt")] Client client, [FromForm(Name = "Photo")] IFormFile? photoFile)
        {
            if (id != client.Id) return NotFound();
            var dbUser = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);
            if (dbUser == null) return NotFound();

            dbUser.UserName = client.UserName;
            dbUser.FirstName = client.FirstName;
            dbUser.LastName = client.LastName;
            dbUser.Email = client.Email;
            dbUser.PhoneNumber = client.PhoneNumber;
            dbUser.Description = client.Description;

            var file = photoFile ?? Request.Form.Files["Photo"];
            var photoBytes = await GetPhotoBytesAsync(file);
            if (photoBytes != null && photoBytes.Length > 0)
            {
                dbUser.PhotoData = photoBytes;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Users.AnyAsync(e => e.Id == client.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();
            var instructor = await _userManager.FindByIdAsync(id);
            if (instructor == null || !await _userManager.IsInRoleAsync(instructor, RoleNames.Instructor))
                return NotFound();
            return View(instructor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var instructor = await _userManager.FindByIdAsync(id);
            if (instructor != null)
            {
                await _userManager.RemoveFromRoleAsync(instructor, RoleNames.Instructor);
                await _userManager.DeleteAsync(instructor);
            }
            return RedirectToAction(nameof(Index));
        }

        // Student: Read-only access to list of instructors
        [Authorize(Roles = RoleNames.CourseStudent)]
        public async Task<IActionResult> List()
        {
            var instructors = await _userManager.GetUsersInRoleAsync(RoleNames.Instructor);
            return View(instructors);
        }

        [Authorize(Roles = RoleNames.CourseStudent)]
        public async Task<IActionResult> DetailsForStudent(string? id)
        {
            if (id == null) return NotFound();
            var instructor = await _userManager.FindByIdAsync(id);
            if (instructor == null || !await _userManager.IsInRoleAsync(instructor, RoleNames.Instructor))
                return NotFound();

            var activeCourses = await _context.StudentCourseInstances
                .Include(s => s.CourseInstances).ThenInclude(c => c.Courses)
                .Include(s => s.Vehicles)
                .Where(s => s.InstructorId == id)
                .OrderByDescending(s => s.CreateAt)
                .ToListAsync();

            ViewBag.ActiveCourses = activeCourses;
            return View(instructor);
        }

        private async Task<byte[]> GetPhotoBytesAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null!;
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
