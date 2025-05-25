using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions.Dynamic;

public class QueryOrder(string field, QueryOrderDirections direction)
{
    [Obsolete("This is only needed on controllers")]
    public QueryOrder() : this(string.Empty, QueryOrderDirections.Asc)
    {
    }

    public QueryOrderDirections Direction { get; set; } = direction;
    public string Field { get; set; } = field;
    public static QueryOrder Asc(string field, params string[] fields)
    {
        var orderField = fields.Aggregate(field, (current, f) => current + $".{f}");
        return new QueryOrder(orderField, QueryOrderDirections.Asc);
    }

    public static QueryOrder Desc(string field, params string[] fields)
    {
        var orderField = fields.Aggregate(field, (current, f) => current + $".{f}");
        return new QueryOrder(orderField, QueryOrderDirections.Desc);
    }

    public IQueryOrder<TEntity> ToQueryOrder<TEntity>()
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var selection = Expression.PropertyOrField(parameter, Field);
    
        // Create a generic lambda with dynamic property type
        var propertyType = selection.Type;
        var funcType = typeof(Func<,>).MakeGenericType(typeof(TEntity), propertyType);
        
        // Use the specific Lambda method that takes Type parameter to avoid ambiguity
        var lambdaMethod = typeof(Expression).GetMethods()
            .Where(m => m.Name == "Lambda" && m.IsGenericMethod && m.GetParameters().Length == 2)
            .First(m => m.GetParameters()[0].ParameterType == typeof(Expression) && 
                        m.GetParameters()[1].ParameterType == typeof(ParameterExpression[]));
        
        var genericLambdaMethod = lambdaMethod.MakeGenericMethod(funcType);
        var lambda = genericLambdaMethod.Invoke(null, new object[] { selection, new[] { parameter } });
    
        // Create the QueryOrder with reflection since we don't know TProperty at compile time
        var orderInfoType = typeof(QueryOrder<,>).MakeGenericType(typeof(TEntity), propertyType);
        return (IQueryOrder<TEntity>)Activator.CreateInstance(orderInfoType, lambda, Direction)!;
    }
    
    public static IQueryOrder<TEntity>? ToQueryOrder<TEntity>(IEnumerable<QueryOrder> orders)
    {
        if (orders.Any() != true)
        {
            return null;
        }
        
        var orderOrder = new Queue<QueryOrder>(orders);
        var orderInfo = orderOrder.Dequeue().ToQueryOrder<TEntity>();

        while (orderOrder.Any())
        {
            var order = orderOrder.Dequeue().ToQueryOrder<TEntity>();
            orderInfo.AddOrderBy(order);
        }

        return orderInfo;
    }
}