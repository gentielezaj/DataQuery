using shlabs.DataQuery.Abstractions;
using shlabs.DataQuery.Abstractions.Dynamic;
using shlabs.DataQuery.Example.Infrastructure.Models;

namespace shlabs.DataQuery.Test;

public class TestQueryOrder : CoreUnitTest
{
    public TestQueryOrder(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void TestOrderBuilder()
    {
        var queryBuilder = new QueryBuilder<Student>();
        queryBuilder.SetOrderBy(x => x.School!.Name, QueryOrderDirections.Asc);
    }
    
    [Fact]
    public void CreateQueryOrderTypedFormDynamic()
    {
        var orderInfoList = new List<QueryOrder>
        {
            new QueryOrder($"{nameof(Student.School)}.{nameof(School.Name)}",
                QueryOrderDirections.Asc),
            new QueryOrder($"{nameof(Student.Name)}",
                QueryOrderDirections.Asc),
        };
        
        var queryOrder = QueryOrder.ToQueryOrder<Student>(orderInfoList);
        Assert.NotNull(queryOrder);
        
        var queryBuilder = new QueryBuilder<Student>();
        queryBuilder.SetOrder(queryOrder!);
    }
}