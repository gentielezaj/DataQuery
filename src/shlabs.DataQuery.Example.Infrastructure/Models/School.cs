using System.ComponentModel.DataAnnotations;

namespace shlabs.DataQuery.Example.Infrastructure.Models
{
    public class School : IdEntity
    {
        [Required]
        public string Name { get; set; }

        // Navigation properties
        public ICollection<Student>? Students { get; set; }
        public ICollection<Teacher>? Teachers { get; set; }
        public ICollection<SchoolClass>? SchoolClasses { get; set; }
    }
}