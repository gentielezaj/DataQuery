using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions.Dynamic;

public class QueryBuilder : CoreQueryBuilder
{
    public QueryBuilderFilter? Filter { get; set; } = new();
    public List<QueryOrder>? Order { get; set; }
    
    public void AddOrder(string field, QueryOrderDirections direction)
    {
        Order ??= new List<QueryOrder>();
        Order.Add(new QueryOrder(field, direction));
    }

    public QueryBuilder<T> ToQueryBuilder<T>()
        where T : class
    {
        return new QueryBuilder<T>
        {
            Order = ToQueryBuilderOrder<T>(),
            Skip = Skip,
            Take = Take,
            Filter = Filter?.ToQueryBuilderFilter<T>()
        };
    }

    private IQueryOrder<T>? ToQueryBuilderOrder<T>()
    {
        if (Order?.Any() != true)
        {
            return null;
        }

        IQueryOrder<T>? orderInfo = null;
        IQueryOrder<T> rootOrder;

        foreach (var item in Order)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression body = parameter;
            foreach (var member in item.Field.Split('.'))
            {
                body = Expression.PropertyOrField(body, member);
            }
            // Box value types to object
            if (body.Type.IsValueType)
                body = Expression.Convert(body, typeof(object));

            var expression = Expression.Lambda<Func<T, object>>(body, parameter);
            var orderInfoInstance = new QueryOrder<T, object>(expression, item.Direction);
            if (orderInfo is null)
            {
                orderInfo = orderInfoInstance;
                rootOrder = orderInfoInstance;
            }
            else
            {
                orderInfo = orderInfo.ThenBy(orderInfoInstance);
            }
        }

        return orderInfo;
    }
}