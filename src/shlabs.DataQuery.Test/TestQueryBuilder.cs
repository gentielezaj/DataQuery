using Microsoft.EntityFrameworkCore;
using shlabs.DataQuery.Abstractions;
using shlabs.DataQuery.Example.Infrastructure.Models;

namespace shlabs.DataQuery.Test;

public class TestDataQuery : CoreUnitTest
{
    public TestDataQuery(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task TestQueryBuilder()
    {
        var queryBuilder = new QueryBuilder<School>();
        var schoolsQuery = queryBuilder.ToQueryable(DbContext);
        var schools = await schoolsQuery.ToListAsync();
        
        Assert.True(schools?.Any() == true, "Schools should not be empty");
    }

    [Fact]
    public async Task TestQueryEntityThenList()
    {
        var studentName = "John Doe";
        var queryBuilder = new QueryBuilder<Teacher>();
        queryBuilder.IncludeEntity(s => s.School)
            .ThenIncludeList(x => x.Students, x => x.Name == studentName);
        var teacherQuery = queryBuilder.ToQueryable(DbContext);
        var teachers = await teacherQuery.ToListAsync();
        Assert.True(teachers?.Any() == true, "Schools should not be empty");
        Assert.All(teachers, teacher =>
        {
            Assert.NotNull(teacher.School?.Students);
            Assert.All(teacher.School.Students, teacher => Assert.Equal(studentName, teacher.Name));
        });
    }
}