using Microsoft.EntityFrameworkCore;
using shlabs.DataQuery.Abstractions;
using shlabs.DataQuery.Abstractions.Dynamic;
using shlabs.DataQuery.Example.Infrastructure.Models;
using static shlabs.DataQuery.Abstractions.Dynamic.QueryBuilderFilter;
using DynamicQueryBuilder = shlabs.DataQuery.Abstractions.Dynamic.QueryBuilder;

namespace shlabs.DataQuery.Test;

public class QueryBuilderSqliteIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public QueryBuilderSqliteIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ToQueryable_applies_appended_and_filters()
    {
        await using var db = _fixture.CreateDbContext();
        var builder = new QueryBuilder<Student>()
            .AppendFilter(student => student.SchoolId == 1)
            .AppendFilter(student => student.Name.Contains("Jane"));

        var students = await builder.ToQueryable(db).ToListAsync();

        var student = Assert.Single(students);
        Assert.Equal(2, student.Id);
        Assert.Equal("Jane Roe", student.Name);
    }

    [Fact]
    public async Task ToQueryable_applies_appended_or_filters()
    {
        await using var db = _fixture.CreateDbContext();
        var builder = new QueryBuilder<Student>()
            .AppendFilter(student => student.Name == "John Doe")
            .AppendFilter(student => student.Name == "Sam Lee", Conditions.Or)
            .AddOrderBy(student => student.Id, QueryOrderDirections.Asc);

        var studentIds = await builder.ToQueryable(db).Select(student => student.Id).ToListAsync();

        Assert.Equal([1, 3], studentIds);
    }

    [Fact]
    public async Task ToQueryable_uses_every_ordering_selector_in_order()
    {
        await using var db = _fixture.CreateDbContext();
        var builder = new QueryBuilder<Student>()
            .AddOrderBy(student => student.SchoolId, QueryOrderDirections.Asc)
            .AddOrderBy(student => student.Name, QueryOrderDirections.Desc);

        var studentNames = await builder.ToQueryable(db).Select(student => student.Name).ToListAsync();

        Assert.Equal(["Sam Lee", "John Doe", "Jane Roe"], studentNames);
    }

    [Fact]
    public async Task ToQueryable_orders_events_by_date_done_order_and_name()
    {
        var date = new DateOnly(2026, 8, 16);
        await using var db = _fixture.CreateDbContext();
        var builder = new QueryBuilder<Event>()
            .AddOrderBy(@event => @event.Date, QueryOrderDirections.Asc)
            .AddOrderBy(@event => @event.IsDone, QueryOrderDirections.Asc)
            .AddOrderBy(@event => @event.Order, QueryOrderDirections.Asc)
            .AddOrderBy(@event => @event.Name, QueryOrderDirections.Asc)
            .AddIncludeEntity(x => x.Student)
            .AppendFilter(x => x.Date >= date || !x.IsDone);

        var actualIds = await builder.ToQueryable(db)
            .Select(@event => @event.Id)
            .ToListAsync();
        var expectedIds = await db.Events
            .Where(x => x.Date >= date || !x.IsDone)
            .Include(x => x.Student)
            .OrderBy(@event => @event.Date)
            .ThenBy(@event => @event.IsDone)
            .ThenBy(@event => @event.Order)
            .ThenBy(@event => @event.Name)
            .Select(@event => @event.Id)
            .ToListAsync();

        Assert.Equal(actualIds.Count, expectedIds.Count);
        Assert.Equal(expectedIds, actualIds);
        for (int i = 0; i < actualIds.Count; i++)
        {
            Assert.True(actualIds[i] == expectedIds[i], $"Event Ids differ at index {i}: actual {actualIds[i]} vs expected {expectedIds[i]}");
        }
    }

    [Fact]
    public async Task ToQueryable_applies_skip_and_take_after_ordering()
    {
        await using var db = _fixture.CreateDbContext();
        var builder = new QueryBuilder<Student>()
            .AddOrderBy(student => student.Id, QueryOrderDirections.Asc)
            .SetSkip(1)
            .SetTake(1);

        var student = Assert.Single(await builder.ToQueryable(db).ToListAsync());

        Assert.Equal(2, student.Id);
    }

    [Fact]
    public async Task ToQueryable_composes_with_an_existing_queryable()
    {
        await using var db = _fixture.CreateDbContext();
        var source = db.Students.Where(student => student.Id > 1);
        var builder = new QueryBuilder<Student>()
            .AppendFilter(student => student.Name.Contains("Sam"));

        var student = Assert.Single(await builder.ToQueryable(source).ToListAsync());

        Assert.Equal(3, student.Id);
    }

    [Fact]
    public async Task ToQueryable_loads_a_filtered_collection_include()
    {
        await using var db = _fixture.CreateDbContext();
        var builder = new QueryBuilder<School>();
        builder.IncludeList(school => school.Students, student => student.Name == "John Doe");

        var schools = await builder.ToQueryable(db).OrderBy(school => school.Id).ToListAsync();

        var centralHigh = Assert.Single(schools, school => school.Id == 1);
        Assert.Collection(
            centralHigh.Students!,
            student => Assert.Equal("John Doe", student.Name));
        Assert.Empty(Assert.Single(schools, school => school.Id == 2).Students!);
    }

    [Fact]
    public async Task ToQueryable_loads_nested_filtered_includes()
    {
        await using var db = _fixture.CreateDbContext();
        var builder = new QueryBuilder<School>();
        builder.IncludeList(school => school.Students, student => student.Name == "John Doe")
            .ThenIncludeList(student => student.Grades);

        var school = Assert.Single(
            await builder.ToQueryable(db).Where(school => school.Id == 1).ToListAsync());
        var student = Assert.Single(school.Students!);

        Assert.Equal("John Doe", student.Name);
        Assert.Equal(["A", "D"], student.Grades!.OrderBy(grade => grade.Id).Select(grade => grade.Value));
    }

    [Fact]
    public async Task Dynamic_query_builder_converts_filters_paging_and_multiple_orders()
    {
        await using var db = _fixture.CreateDbContext();
        var dynamicBuilder = new DynamicQueryBuilder
        {
            Filter = QueryBuilderFilter.And(
                new QueryBuilderFilterCriteria("SchoolId", QueryBuilderCriteriaConditions.Equal, "1"),
                new QueryBuilderFilterCriteria("Name", QueryBuilderCriteriaConditions.Contains, "e")),
            Skip = 1,
            Take = 1
        };
        dynamicBuilder.AddOrder("SchoolId", QueryOrderDirections.Asc);
        dynamicBuilder.AddOrder("Name", QueryOrderDirections.Asc);

        var students = await dynamicBuilder.ToQueryBuilder<Student>().ToQueryable(db).ToListAsync();

        var student = Assert.Single(students);
        Assert.Equal("John Doe", student.Name);
    }
}
