using shlabs.DataQuery.Abstractions.Utils;
using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions;

public interface IQueryIncludeList;

public interface IQueryIncludeList<TEntity> : IQueryInclude<TEntity>, IQueryIncludeList 
    where TEntity : class;

public class QueryIncludeList<TEntity, TProperty>(
    Expression<Func<TEntity, IEnumerable<TProperty>?>> navigation,
    Expression<Func<TProperty, bool>>? filter,
    IThenQueryInclude<TProperty>? thenIncludes = null)
    : CoreQueryIncludeList<TEntity, TProperty>(navigation, filter, thenIncludes), IQueryIncludeList<TEntity>
    where TEntity : class
    where TProperty : class
{
    public QueryIncludeList(Expression<Func<TEntity, ICollection<TProperty>?>> navigation, IThenQueryInclude<TProperty>? thenIncludes = null)
        : this(navigation.ToEnumerableExpression(), null, thenIncludes)
    {
    }
}