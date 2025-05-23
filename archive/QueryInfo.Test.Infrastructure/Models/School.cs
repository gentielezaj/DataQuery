using QueryInfo.Test.Infrastructure.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QueryInfo.Test.Infrastructure.Model
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