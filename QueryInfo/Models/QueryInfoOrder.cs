using QueryInfo.Utilities;
using System.Linq.Expressions;

namespace QueryInfo.Models;

public interface IOrderInfo<TEntity>
{
    OrderInfoDirections Direction { get; }

    IOrderInfo<TEntity>? ThenOrderBy { get; }

    IOrderInfo<TEntity> ThenBy(IOrderInfo<TEntity> thenBy);

    IQueryable<TEntity> ToQueryable(IQueryable<TEntity> source);

    IEnumerable<KeyValuePair<string, OrderInfoDirections>> GetOrderList();
}

public class OrderInfo<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> selector, OrderInfoDirections direction)
    : IOrderInfo<TEntity>
{
    public Expression<Func<TEntity, TProperty>> Selector { get; init; } = selector;
    public OrderInfoDirections Direction { get; init; } = direction;
    public IOrderInfo<TEntity>? ThenOrderBy { get; private set; } = null;

    public OrderInfo<TEntity, TNextProperty> ThenBy<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector, OrderInfoDirections direction)
    {
        var orderInfo = new OrderInfo<TEntity, TNextProperty>(selector, direction);
        ThenOrderBy = orderInfo;
        return orderInfo;
    }

    public IOrderInfo<TEntity> ThenBy(IOrderInfo<TEntity> thenBy)
    {
        ThenOrderBy = thenBy;
        return ThenOrderBy;
    }

    public OrderInfo<TEntity, TNextProperty> ThenByAsc<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector)
    {
        return ThenBy(selector, OrderInfoDirections.Asc);
    }

    public OrderInfo<TEntity, TNextProperty> ThenByDesc<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector)
    {
        return ThenBy(selector, OrderInfoDirections.Desc);
    }

    public IQueryable<TEntity> ToQueryable(IQueryable<TEntity> source)
    {
        var orderInfos = GetOrderList();
        if (orderInfos.Any() != true)
        {
            return source;
        }

        var expression = source.Expression;
        var count = 0;
        foreach (var item in orderInfos)
        {
            var paths = item.Key.Split(".");
            foreach (var path in paths)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");
                Expression selector = Expression.PropertyOrField(parameter, path);

                var methodName = Equals(item.Value, OrderInfoDirections.Desc)
                    ? (count == 0 ? "OrderByDescending" : "ThenByDescending")
                    : (count == 0 ? "OrderBy" : "ThenBy");

                expression = Expression.Call(typeof(Queryable), methodName,
                    [source.ElementType, selector.Type],
                    expression, Expression.Quote(Expression.Lambda(selector, parameter)));
                count++;
            }
        }

        return count > 0 ? source.Provider.CreateQuery<TEntity>(expression) : source;
    }

    public IEnumerable<KeyValuePair<string, OrderInfoDirections>> GetOrderList()
    {
        var field = ExpressionUtils.GetPropertyPath(Selector);

        var list = new List<KeyValuePair<string, OrderInfoDirections>> { new(field, Direction) };
        if (ThenOrderBy is not null)
        {
            list.AddRange(ThenOrderBy.GetOrderList());
        }
        return list;
    }

    public IQueryable<TEntity> ToQueryableNotSupported(IQueryable<TEntity> source)
    {
        IOrderedQueryable<TEntity>? orderedQueryable = null;
        if (source is IOrderedQueryable<TEntity> order)
        {
            orderedQueryable = Direction == OrderInfoDirections.Asc
                ? order.ThenBy(Selector)
                : order.ThenByDescending(Selector);
        }
        else
        {
            orderedQueryable = Direction == OrderInfoDirections.Asc
                ? source.OrderBy(Selector)
                : source.OrderByDescending(Selector);
        }

        if (ThenOrderBy is not null)
        {
            return ThenOrderBy.ToQueryable(orderedQueryable);
        }

        return orderedQueryable;
    }
}

public class OrderInfo(string field, OrderInfoDirections direction)
{
    [Obsolete("This is only needed on controllers")]
    public OrderInfo() : this(string.Empty, OrderInfoDirections.Asc)
    {
    }

    public OrderInfoDirections Direction { get; set; } = direction;
    public string Field { get; set; } = field;

    public static OrderInfo Asc(string field, params string[] fields)
    {
        var orderField = fields.Aggregate(field, (current, f) => current + $".{f}");
        return new OrderInfo(orderField, OrderInfoDirections.Asc);
    }

    public static OrderInfo Desc(string field, params string[] fields)
    {
        var orderField = fields.Aggregate(field, (current, f) => current + $".{f}");
        return new OrderInfo(orderField, OrderInfoDirections.Desc);
    }
}

public enum OrderInfoDirections
{
    Asc,
    Desc
}
