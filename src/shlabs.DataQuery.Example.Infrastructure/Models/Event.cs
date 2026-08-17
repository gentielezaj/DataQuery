using System.ComponentModel.DataAnnotations.Schema;

namespace shlabs.DataQuery.Example.Infrastructure.Models
{
    public class Event : IdEntity
    {
        public string Name { get; set; }
        public DateOnly Date { get; set; }
        public bool IsDone { get; set; }
        public int Order { get; set; }
        // Foreign keys
        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Student? Student { get; set; }
    }
}
