using QueryInfo.Test.Infrastructure.Models;

namespace QueryInfo.Test.Infrastructure.Model
{
    public class Teacher : IdNameSchoolEntity
    {
        // Navigation
        public ICollection<SchoolClass> SchoolClasses { get; set; }
    }
}