using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions;

public interface IQueryIncludeList<TEntity> : IQueryInclude<TEntity> 
    where TEntity : class;

public class QueryIncludeList<TEntity, TProperty>(
    Expression<Func<TEntity, IEnumerable<TProperty>?>> navigation,
    Expression<Func<TProperty, bool>>? filter,
    IThenQueryInclude<TProperty>? thenIncludes = null)
    : CoreQueryIncludeList<TEntity, TProperty>(navigation, filter, thenIncludes), IQueryIncludeList<TEntity>
    where TEntity : class
    where TProperty : class;