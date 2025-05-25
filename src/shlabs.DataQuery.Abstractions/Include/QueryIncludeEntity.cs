using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions;

public interface IQueryIncludeEntity<TEntity> : IQueryInclude<TEntity>
    where TEntity : class;

public class QueryIncludeEntity<TEntity, TProperty>(
    Expression<Func<TEntity, TProperty?>> navigation,
    IThenQueryInclude<TProperty>? thenIncludes = null)
    : CoreQueryInclude<TEntity, TProperty, TProperty>(navigation, thenIncludes), IQueryIncludeEntity<TEntity>
    where TEntity : class
    where TProperty : class
{
    public QueryIncludeEntity(IQueryIncludeEntity<TEntity> queryInclude)
        : this(null!, null)
    {
        if (queryInclude is QueryIncludeEntity<TEntity, TProperty> queryIncludeEntity)
        {
            Navigation = queryIncludeEntity.Navigation;
            ThenIncludes = queryIncludeEntity.ThenIncludes;
        }
        
        throw new ArgumentException("Invalid query include type");
    }
}