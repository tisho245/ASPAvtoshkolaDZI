using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Avtoshkola_DZI.Data
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }   
        public string Description { get; set; }
        public ICollection<Course> Courses { get; set; }
    }
}
