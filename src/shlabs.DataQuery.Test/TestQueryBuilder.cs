using Microsoft.EntityFrameworkCore;
using shabs.DataQuery.Abstractions;
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
}