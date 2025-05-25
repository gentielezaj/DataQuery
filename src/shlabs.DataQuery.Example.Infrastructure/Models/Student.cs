namespace shlabs.DataQuery.Example.Infrastructure.Models
{
    public class Student : IdNameSchoolEntity
    {
        // Navigation
        public ICollection<Grade> Grades { get; set; }
    }
}