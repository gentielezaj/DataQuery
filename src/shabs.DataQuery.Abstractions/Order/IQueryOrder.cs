using System.Linq.Expressions;

namespace shabs.DataQuery.Abstractions;

public interface IQueryOrder<TEntity>
{
    LambdaExpression LambdaSelector { get; }

    QueryOrderDirections Direction { get; }

    IQueryOrder<TEntity>? ThenOrderBy { get; }

    IQueryOrder<TEntity> ThenBy(IQueryOrder<TEntity> thenBy);

    QueryOrder<TEntity, TNextProperty> ThenByAsc<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector);

    QueryOrder<TEntity, TNextProperty> ThenByDesc<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector);

    QueryOrder<TEntity, TNextProperty> ThenBy<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector,
        QueryOrderDirections direction);

    IQueryOrder<TEntity> AddOrderBy(IQueryOrder<TEntity>? thenBy);
    IQueryOrder<TEntity> AddOrderBy(IEnumerable<IQueryOrder<TEntity>>? thenBy);

    IEnumerable<IQueryOrder<TEntity>> GetOrderAsList();
}