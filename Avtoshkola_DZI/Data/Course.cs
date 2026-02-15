using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Avtoshkola_DZI.Data
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int TotalTheoryHours { get; set; }
        public int TotalPracticeHours { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category Categories { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<CourseInstance> CourseInstances { get; set; }
    }
}
