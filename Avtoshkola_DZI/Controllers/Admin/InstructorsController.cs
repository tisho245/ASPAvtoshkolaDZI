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
    public class InstructorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;
        private readonly PhotoUploadService _photoUpload;

        public InstructorsController(ApplicationDbContext context, UserManager<Client> userManager, PhotoUploadService photoUpload)
        {
            _context = context;
            _userManager = userManager;
            _photoUpload = photoUpload;
        }

        public async Task<IActionResult> Index()
        {
            var roleId = await _context.Roles.Where(r => r.Name == RoleNames.Instructor).Select(r => r.Id).FirstOrDefaultAsync();
            if (roleId == null) return View(new List<Client>());
            var userIds = await _context.UserRoles.Where(ur => ur.RoleId == roleId).Select(ur => ur.UserId).ToListAsync();
            var instructors = await _context.Users.OfType<Client>()
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
                    Description = c.Description,
                    QualificationDesc = c.QualificationDesc,
                    LicenseNumber = c.LicenseNumber
                })
                .ToListAsync();
            return View(instructors);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);
            if (client == null || !await _userManager.IsInRoleAsync(client, RoleNames.Instructor))
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
        public async Task<IActionResult> Create([Bind("UserName,FirstName,LastName,Email,PhoneNumber,Description,QualificationDesc,LicenseNumber")] Client client, string? password, [FromForm(Name = "Photo")] IFormFile? photoFile)
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

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);
            if (client == null || !await _userManager.IsInRoleAsync(client, RoleNames.Instructor))
                return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,FirstName,LastName,Email,PhoneNumber,Description,QualificationDesc,LicenseNumber,CreatedAt")] Client client, [FromForm(Name = "Photo")] IFormFile? photoFile)
        {
            if (id != client.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _userManager.FindByIdAsync(id);
                if (existing == null) return NotFound();
                existing.FirstName = client.FirstName;
                existing.LastName = client.LastName;
                existing.Email = client.Email;
                existing.UserName = client.UserName;
                existing.PhoneNumber = client.PhoneNumber;
                existing.Description = client.Description;
                byte[]? photoBytes = null;
                var file = photoFile ?? Request.Form.Files["Photo"];
                if (file != null && file.Length > 0)
                {
                    photoBytes = await _photoUpload.GetPhotoBytesAsync(file);
                    if (photoBytes != null) existing.PhotoData = photoBytes;
                }
                existing.QualificationDesc = client.QualificationDesc;
                existing.LicenseNumber = client.LicenseNumber;
                await _userManager.UpdateAsync(existing);
                if (photoBytes != null && photoBytes.Length > 0)
                {
                    var dbUser = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == id);
                    if (dbUser != null)
                    {
                        dbUser.PhotoData = photoBytes;
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(client);
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null) return NotFound();
            var client = await _userManager.FindByIdAsync(id);
            if (client == null || !await _userManager.IsInRoleAsync(client, RoleNames.Instructor))
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
                await _userManager.RemoveFromRoleAsync(client, RoleNames.Instructor);
                await _userManager.DeleteAsync(client);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
