using System.ComponentModel.DataAnnotations;
using Avtoshkola_DZI.Models;
using Avtoshkola_DZI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Controllers
{
    [AllowAnonymous]
    public class RegisterStudentController : Controller
    {
        private readonly UserManager<Client> _userManager;
        private readonly SignInManager<Client> _signInManager;
        private readonly PhotoUploadService _photoUpload;
        private readonly ApplicationDbContext _context;

        public RegisterStudentController(UserManager<Client> userManager, SignInManager<Client> signInManager, PhotoUploadService photoUpload, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _photoUpload = photoUpload;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Index(RegisterStudentInputModel model, [FromForm(Name = "Photo")] IFormFile? photoFile)
        {
            if (!ModelState.IsValid)
            {
                // Показваме валидаторските грешки във формата
                return View(model);
            }

            var file = photoFile ?? Request.Form.Files["Photo"];
            var photoBytes = await _photoUpload.GetPhotoBytesAsync(file);

            var now = DateTime.UtcNow;
            var client = new Client
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                PhotoData = photoBytes,
                Description = "",
                CreatedAt = now,
                LastSignedIn = now,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(client, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

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

            await _signInManager.SignInAsync(client, isPersistent: false);
            return RedirectToAction("Index", "Home", new { area = "Student" });
        }
    }

    public class RegisterStudentInputModel
    {
        [Required(ErrorMessage = "Потребителското име е задължително")]
        [Display(Name = "Потребителско име")]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "Име е задължително")]
        [Display(Name = "Име")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Фамилия е задължителна")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "E-mail е задължителен")]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = "";

        [Display(Name = "Телефон")]
        public string? PhoneNumber { get; set; }


        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Потвърди парола")]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
