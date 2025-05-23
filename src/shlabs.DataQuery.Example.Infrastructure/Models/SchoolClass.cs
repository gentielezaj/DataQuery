using QueryInfo.Test.Infrastructure.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QueryInfo.Test.Infrastructure.Model
{
    public class SchoolClass : IdNameSchoolEntity
    {
        public int TeacherId { get; set; }
        [ForeignKey(nameof(TeacherId))]
        public Teacher? Teacher { get; set; }

        // Navigation
        public ICollection<Grade>? Grades { get; set; }
    }
}