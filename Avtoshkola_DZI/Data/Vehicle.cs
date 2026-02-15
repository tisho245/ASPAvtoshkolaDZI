using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Avtoshkola_DZI.Data
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public Category Categories { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public bool IsAutomatic { get; set; }
        public int Year { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<StudentCourseInstance> StudentCourseInstances { get; set; }
    }
}
