using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Avtoshkola_DZI.Data
{
    public class StudentCourseInstance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicles { get; set; }
        
        public string  StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Client Student { get; set; }
        public string InstructorId { get; set; }
        [ForeignKey(nameof(InstructorId))]
        public Client Instructor { get; set; }
        public DateTime CreateAt { get; set; }
        public int CourseInstanceId { get; set; }
        public CourseInstance CourseInstances { get; set; }
        public int CurrentTheoryHours { get; set; }
        public int CurrentPracticeHours { get; set; }   
    }
}
