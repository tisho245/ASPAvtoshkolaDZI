using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Avtoshkola_DZI.Data
{
    public class Client:IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public DateTime LastSignedIn { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PhotoURL { get; set; }
        public string? EGN { get; set; }
        public string? LicenseNumber { get; set; }
        public string? QualificationDesc { get; set; }
        public string Description { get; set; }
        public ICollection<StudentCourseInstance> StudentCourseInstances { get; set; }
        public ICollection<StudentCourseInstance> InstructorCourseInstances { get; set; }
    }
}
