using Microsoft.EntityFrameworkCore;
using QueryInfo.Test.Infrastructure.Model;

namespace shlabs.DataQuery.Example.Infrastructure.Config;

public class AddDataToTable
{
    public static void AddDataToTableD(ModelBuilder modelBuilder)
    {
        // Seed Schools
        modelBuilder.Entity<School>().HasData(
            new School { Id = 1, Name = "Central High" },
            new School { Id = 2, Name = "Westside Elementary" }
        );

        // Seed Teachers
        modelBuilder.Entity<Teacher>().HasData(
            new Teacher { Id = 1, Name = "Alice Smith", SchoolId = 1 },
            new Teacher { Id = 2, Name = "Bob Johnson", SchoolId = 1 }
        );

        // Seed Students
        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, Name = "John Doe", SchoolId = 1 },
            new Student { Id = 2, Name = "Jane Roe", SchoolId = 1 },
            new Student { Id = 3, Name = "Sam Lee", SchoolId = 1 }
        );

        // Seed SchoolClasses
        modelBuilder.Entity<SchoolClass>().HasData(
            new SchoolClass { Id = 1, Name = "Math 101", SchoolId = 1, TeacherId = 1 },
            new SchoolClass { Id = 2, Name = "Science 201", SchoolId = 1, TeacherId = 2 }
        );

        // Seed Grades
        modelBuilder.Entity<Grade>().HasData(
            new Grade { Id = 1, Value = "A", StudentId = 1, SchoolClassId = 1 },
            new Grade { Id = 2, Value = "B", StudentId = 2, SchoolClassId = 1 },
            new Grade { Id = 3, Value = "C", StudentId = 3, SchoolClassId = 1 },
            new Grade { Id = 4, Value = "D", StudentId = 1, SchoolClassId = 2 },
            new Grade { Id = 5, Value = "F", StudentId = 2, SchoolClassId = 2 },
            new Grade { Id = 6, Value = "G", StudentId = 3, SchoolClassId = 2 }
        );
    }
}