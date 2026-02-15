using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Avtoshkola_DZI.Data
{
    public class CourseInstance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; }
        public Course Courses { get; set; }
        public ICollection<StudentCourseInstance> StudentCourseInstances { get;set; }
    }
}
