using QueryInfo.Test.Infrastructure.Models;

namespace QueryInfo.Test.Infrastructure.Model
{
    public class Student : IdNameSchoolEntity
    {
        // Navigation
        public ICollection<Grade> Grades { get; set; }
    }
}