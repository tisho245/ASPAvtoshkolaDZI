#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avtoshkola_DZI.Data;
using Avtoshkola_DZI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avtoshkola_DZI.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly UserManager<Client> _userManager;
        private readonly PhotoUploadService _photoUpload;

        public EditModel(UserManager<Client> userManager, PhotoUploadService photoUpload)
        {
            _userManager = userManager;
            _photoUpload = photoUpload;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Име")]
            [Required(ErrorMessage = "Полето Име е задължително.")]
            [StringLength(100)]
            public string FirstName { get; set; }

            [Display(Name = "Фамилия")]
            [Required(ErrorMessage = "Полето Фамилия е задължително.")]
            [StringLength(100)]
            public string LastName { get; set; }

            [Display(Name = "Телефон")]
            [Phone(ErrorMessage = "Невалиден телефонен номер.")]
            [StringLength(20)]
            public string PhoneNumber { get; set; }

            [Display(Name = "Описание")]
            [StringLength(500)]
            public string Description { get; set; }

            [Display(Name = "Снимка")]
            public IFormFile Photo { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Description = user.Description
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.PhoneNumber = Input.PhoneNumber;
            user.Description = Input.Description ?? "";

            if (Input.Photo != null && Input.Photo.Length > 0)
            {
                var photoBytes = await _photoUpload.GetPhotoBytesAsync(Input.Photo);
                if (photoBytes != null && photoBytes.Length > 0)
                {
                    user.PhotoData = photoBytes;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["ProfileMessage"] = "Профилът е обновен успешно.";
                return RedirectToPage("./Index");
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
