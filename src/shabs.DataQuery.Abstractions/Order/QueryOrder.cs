using System.Linq.Expressions;

namespace shabs.DataQuery.Abstractions;

public class QueryOrder<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> selector, QueryOrderDirections direction)
    : IQueryOrder<TEntity>
{
    public Expression<Func<TEntity, TProperty>> Selector { get; init; } = selector;
    public LambdaExpression LambdaSelector => Selector;
    public QueryOrderDirections Direction { get; init; } = direction;
    public IQueryOrder<TEntity>? ThenOrderBy { get; private set; } = null;

    public QueryOrder<TEntity, TNextProperty> ThenBy<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector,
        QueryOrderDirections direction)
    {
        var orderInfo = new QueryOrder<TEntity, TNextProperty>(selector, direction);
        ThenOrderBy = orderInfo;
        return orderInfo;
    }

    public IQueryOrder<TEntity> ThenBy(IQueryOrder<TEntity> thenBy)
    {
        ThenOrderBy = thenBy;
        return ThenOrderBy;
    }

    public QueryOrder<TEntity, TNextProperty> ThenByAsc<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector)
    {
        return ThenBy(selector, QueryOrderDirections.Asc);
    }

    public QueryOrder<TEntity, TNextProperty> ThenByDesc<TNextProperty>(
        Expression<Func<TEntity, TNextProperty>> selector)
    {
        return ThenBy(selector, QueryOrderDirections.Desc);
    }

    public IQueryOrder<TEntity> AddOrderBy(IQueryOrder<TEntity>? thenBy)
        => AddOrderBy(thenBy is null ? null : [thenBy]);

    public IQueryOrder<TEntity> AddOrderBy(IEnumerable<IQueryOrder<TEntity>>? thenBy)
    {
        if (thenBy?.Any() != true)
        {
            return this;
        }

        if (ThenOrderBy is not null)
        {
            return ThenOrderBy.AddOrderBy(thenBy);
        }

        var q = new Queue<IQueryOrder<TEntity>>(thenBy);
        ThenOrderBy = q.Dequeue();
        return ThenOrderBy.AddOrderBy(q.ToArray());
    }

    public IEnumerable<IQueryOrder<TEntity>> GetOrderAsList()
    {
        var list = new List<IQueryOrder<TEntity>> { this };
        if (ThenOrderBy is not null)
        {
            list.AddRange(ThenOrderBy.GetOrderAsList());
        }

        return list;
    }
}