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

        Assert.NotNull(queryBuilder.Order);
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
        var queryBuilder = new QueryBuilder<Student>();
        queryBuilder.SetOrder(queryOrder!);
        Assert.NotNull(queryBuilder.Order?.ThenOrderBy);
    }

    [Fact]
    public void CreateQueryOrderTypedFormDynamicOne()
    {
        var orderInfoList = new List<IQueryOrder<Student>>
        {
            new QueryOrder($"{nameof(Student.School)}.{nameof(School.Name)}",
                QueryOrderDirections.Asc).ToQueryOrder<Student>(),
            new QueryOrder($"{nameof(Student.Name)}",
                QueryOrderDirections.Asc).ToQueryOrder<Student>(),
        };

        var queryBuilder = new QueryBuilder<Student>();
        queryBuilder.SetOrder(orderInfoList.ToArray());

        Assert.NotNull(queryBuilder.Order?.ThenOrderBy);
    }
}