#nullable disable

using System.Threading.Tasks;
using Avtoshkola_DZI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<Client> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<Client> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EGN { get; set; }
        public string LicenseNumber { get; set; }
        public string QualificationDesc { get; set; }
        public string Description { get; set; }
        public string CreatedAt { get; set; }
        public string RoleLabel { get; set; }
        public string PhotoDataUrl { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            var user = await _context.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == userId);
            if (user == null)
                return NotFound();

            UserId = user.Id;
            UserName = user.UserName;
            if (user.PhotoData != null && user.PhotoData.Length > 0)
            {
                var ct = "image/jpeg";
                if (user.PhotoData.Length >= 4 && user.PhotoData[0] == 0x89 && user.PhotoData[1] == 0x50 && user.PhotoData[2] == 0x4E && user.PhotoData[3] == 0x47) ct = "image/png";
                else if (user.PhotoData.Length >= 3 && user.PhotoData[0] == 0x47 && user.PhotoData[1] == 0x49 && user.PhotoData[2] == 0x46) ct = "image/gif";
                else if (user.PhotoData.Length >= 12 && user.PhotoData[0] == 0x52 && user.PhotoData[1] == 0x49 && user.PhotoData[2] == 0x46 && user.PhotoData[3] == 0x46 && user.PhotoData[8] == 0x57 && user.PhotoData[9] == 0x45 && user.PhotoData[10] == 0x42 && user.PhotoData[11] == 0x50) ct = "image/webp";
                PhotoDataUrl = "data:" + ct + ";base64," + Convert.ToBase64String(user.PhotoData);
            }
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            PhoneNumber = user.PhoneNumber;
            EGN = user.EGN;
            LicenseNumber = user.LicenseNumber;
            QualificationDesc = user.QualificationDesc;
            Description = user.Description;
            CreatedAt = user.CreatedAt.ToString("dd.MM.yyyy");

            if (User.IsInRole(RoleNames.Administrator)) RoleLabel = "Администратор";
            else if (User.IsInRole(RoleNames.Instructor)) RoleLabel = "Инструктор";
            else if (User.IsInRole(RoleNames.Student)) RoleLabel = "Курсист";
            else RoleLabel = "Потребител";

            return Page();
        }
    }
}
