using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Avtoshkola_DZI.Models
{
    public class Client : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public DateTime LastSignedIn { get; set; }
        public DateTime CreatedAt { get; set; }
        /// <summary>Снимка в двоичен формат. Зарежда се само при GetById (Details); не се зарежда при списъци.</summary>
        public byte[]? PhotoData { get; set; }
        public string? EGN { get; set; }
        public string? LicenseNumber { get; set; }
        public string? QualificationDesc { get; set; }
        // Описанието да не е задължително в моделите/формите
        public string? Description { get; set; }
        public ICollection<StudentCourseInstance> StudentCourseInstances { get; set; } = new List<StudentCourseInstance>();
        public ICollection<StudentCourseInstance> InstructorCourseInstances { get; set; } = new List<StudentCourseInstance>();
    }
}
