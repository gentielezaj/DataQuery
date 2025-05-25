using System.ComponentModel.DataAnnotations.Schema;

namespace shlabs.DataQuery.Example.Infrastructure.Models
{
    public class Grade : IdEntity
    {
        public string Value { get; set; }

        // Foreign keys
        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Student? Student { get; set; }

        public int SchoolClassId { get; set; }
        [ForeignKey(nameof(SchoolClassId))]
        public SchoolClass? SchoolClass { get; set; }
    }
}