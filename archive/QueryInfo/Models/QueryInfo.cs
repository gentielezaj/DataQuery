using System.Linq.Expressions;

namespace QueryInfo.Models;

public class QueryInfo : CoreQueryInfo
{
    public QueryInfoFilter? Filter { get; set; } = new();
    public List<OrderInfo>? Order { get; set; }

    public void AddOrder(string field, OrderInfoDirections direction)
    {
        Order ??= new List<OrderInfo>();
        Order.Add(new OrderInfo(field, direction));
    }

    public QueryInfo<T> ToQueryInfo<T>()
        where T : class
    {
        return new QueryInfo<T>
        {
            OrderInfo = ToOrderInfo<T>(),
            Skip = Skip,
            Take = Take,
            Where = Filter?.ToQueryInfoFilter<T>()
        };
    }

    private IOrderInfo<T>? ToOrderInfo<T>()
    {
        if (Order?.Any() != true)
        {
            return null;
        }

        IOrderInfo<T>? orderInfo = null;
        IOrderInfo<T> rootOrder;

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
            var orderInfoInstance = new OrderInfo<T, object>(expression, item.Direction);
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

