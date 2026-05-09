#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avtoshkola_DZI.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<Client> _userManager;
        private readonly SignInManager<Client> _signInManager;

        public ChangePasswordModel(UserManager<Client> userManager, SignInManager<Client> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Текуща парола")]
            [Required(ErrorMessage = "Въведете текущата парола.")]
            [DataType(DataType.Password)]
            public string OldPassword { get; set; }

            [Display(Name = "Нова парола")]
            [Required(ErrorMessage = "Въведете нова парола.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да е между {2} и {1} символа.")]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [Display(Name = "Потвърди нова парола")]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Паролите не съвпадат.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid) return Page();

            var changeResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (changeResult.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["ProfileMessage"] = "Паролата е сменена успешно.";
                return RedirectToPage("./Index");
            }
            foreach (var error in changeResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }
    }
}
