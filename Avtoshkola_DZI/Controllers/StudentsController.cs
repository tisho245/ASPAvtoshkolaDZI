using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        public StudentsController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Admin: Full CRUD access to all students
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Index()
        {
            var roleId = await _context.Roles.Where(r => r.Name == RoleNames.CourseStudent).Select(r => r.Id).FirstOrDefaultAsync();
            if (roleId == null) return View(new List<Client>());
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == roleId).Select(ur => ur.UserId).ToListAsync();
            var students = await _context.Users.OfType<Client>()
                .Where(c => userIds.Contains(c.Id))
                .Select(c => new Client
                {
                    Id = c.Id,
                    UserName = c.UserName,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Description = c.Description ?? ""
                })
                .ToListAsync();
            return View(students);
        }

        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);
            if (client == null || !await _userManager.IsInRoleAsync(client, RoleNames.CourseStudent))
                return NotFound();
            return View(client);
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
                    await _userManager.AddToRoleAsync(client, RoleNames.CourseStudent);
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
            var client = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);
            if (client == null || !await _userManager.IsInRoleAsync(client, RoleNames.CourseStudent))
                return NotFound();
            return View(client);
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
                return RedirectToAction("Details", "Students", new { area = "Admin", id });
            }
            return View(client);
        }

        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();
            var client = await _userManager.FindByIdAsync(id);
            if (client == null || !await _userManager.IsInRoleAsync(client, RoleNames.CourseStudent))
                return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.Administrator)]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var client = await _userManager.FindByIdAsync(id);
            if (client != null)
            {
                await _userManager.RemoveFromRoleAsync(client, RoleNames.CourseStudent);
                await _userManager.DeleteAsync(client);
            }
            return RedirectToAction(nameof(Index));
        }

        // Instructor: Read-only access to their assigned students
        [Authorize(Roles = RoleNames.Instructor)]
        public async Task<IActionResult> MyStudents()
        {
            var instructorId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(instructorId))
                return RedirectToAction("Index", "Home", new { area = "Instructor" });

            var students = await _context.StudentCourseInstances
                .Include(s => s.Student)
                .Where(s => s.InstructorId == instructorId)
                .Select(s => s.Student)
                .Distinct()
                .ToListAsync();

            return View(students);
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
