using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace shlabs.DataQuery.Example.Infrastructure.Models
{
    public abstract class IdEntity
    {
        [Key]
        public int Id { get; set; }
    }

    public abstract class IdSchoolEntity : IdEntity
    {
        [Required]
        public int SchoolId { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public School? School { get; set; }
    }

    public abstract class IdNameSchoolEntity : IdSchoolEntity
    {
        [Required]
        [StringLength(64)]
        public string Name { get; set; }
    }
}
