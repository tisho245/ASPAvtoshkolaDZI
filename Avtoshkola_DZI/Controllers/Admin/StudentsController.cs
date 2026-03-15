using Avtoshkola_DZI.Data;
using Avtoshkola_DZI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers.Admin
{
    [Authorize(Roles = RoleNames.Administrator)]
    [Area("Admin")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;
        private readonly PhotoUploadService _photoUpload;

        public StudentsController(ApplicationDbContext context, UserManager<Client> userManager, PhotoUploadService photoUpload)
        {
            _context = context;
            _userManager = userManager;
            _photoUpload = photoUpload;
        }

        public async Task<IActionResult> Index()
        {
            var roleId = await _context.Roles.Where(r => r.Name == RoleNames.Student).Select(r => r.Id).FirstOrDefaultAsync();
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
                    CreatedAt = c.CreatedAt,
                    Description = c.Description ?? ""
                })
                .ToListAsync();
            return View(students);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);
            if (client == null || !await _userManager.IsInRoleAsync(client, RoleNames.Student))
                return NotFound();
            return View(client);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Create([Bind("UserName,FirstName,LastName,Email,PhoneNumber,Description")] Client client, string? password, [FromForm(Name = "Photo")] IFormFile? photoFile)
        {
            if (string.IsNullOrEmpty(password)) password = "Test123!";
            client.CreatedAt = DateTime.UtcNow;
            client.LastSignedIn = client.CreatedAt;
            client.EmailConfirmed = true;
            client.Description ??= "";
            var file = photoFile ?? Request.Form.Files["Photo"];
            var photoBytes = await _photoUpload.GetPhotoBytesAsync(file);
            client.PhotoData = photoBytes;
            if (ModelState.IsValid)
            {
                var result = await _userManager.CreateAsync(client, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(client, RoleNames.Student);
                    if (photoBytes != null && photoBytes.Length > 0)
                    {
                        var dbUser = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == client.Id);
                        if (dbUser != null)
                        {
                            dbUser.PhotoData = photoBytes;
                            await _context.SaveChangesAsync();
                        }
                    }
                    // След създаване – към списъка с всички курсисти в Admin
                    return RedirectToAction("Index", "Students", new { area = "Admin" });
                }
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
            }
            return View(client);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);
            if (client == null || !await _userManager.IsInRoleAsync(client, RoleNames.Student))
                return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,FirstName,LastName,Email,PhoneNumber,Description,CreatedAt")] Client client, [FromForm(Name = "Photo")] IFormFile? photoFile)
        {
            if (id != client.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                return View(client);
            }

            var existing = await _userManager.FindByIdAsync(id);
            if (existing == null) return NotFound();

            existing.FirstName = client.FirstName;
            existing.LastName = client.LastName;
            existing.Email = client.Email;
            existing.UserName = client.UserName;
            existing.PhoneNumber = client.PhoneNumber;
            existing.Description = client.Description ?? "";

            byte[]? photoBytes = null;
            var file = photoFile ?? Request.Form.Files["Photo"];
            if (file != null && file.Length > 0)
            {
                photoBytes = await _photoUpload.GetPhotoBytesAsync(file);
                if (photoBytes != null) existing.PhotoData = photoBytes;
            }

            var result = await _userManager.UpdateAsync(existing);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(client);
            }

            if (photoBytes != null && photoBytes.Length > 0)
            {
                var dbUser = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);
                if (dbUser != null)
                {
                    dbUser.PhotoData = photoBytes;
                    await _context.SaveChangesAsync();
                }
            }

            // След редактиране – към детайли за конкретния курсист в Admin
            return RedirectToAction("Details", "Students", new { area = "Admin", id });
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();
            var client = await _userManager.FindByIdAsync(id);
            if (client == null || !await _userManager.IsInRoleAsync(client, RoleNames.Student))
                return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var client = await _userManager.FindByIdAsync(id);
            if (client != null)
            {
                await _userManager.RemoveFromRoleAsync(client, RoleNames.Student);
                await _userManager.DeleteAsync(client);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
