namespace shlabs.DataQuery.Example.Infrastructure.Models
{
    public class Teacher : IdNameSchoolEntity
    {
        // Navigation
        public ICollection<SchoolClass> SchoolClasses { get; set; }
    }
}