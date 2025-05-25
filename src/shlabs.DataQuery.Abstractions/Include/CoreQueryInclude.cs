using System.Collections;
using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions;

public abstract class CoreQueryInclude<TEntity, TProperty, TPropertyUnderlying>(
    Expression<Func<TEntity, TProperty?>> navigation,
    IThenQueryInclude<TPropertyUnderlying>? thenIncludes = null) : IQueryInclude<TEntity>
    where TEntity : class
    where TPropertyUnderlying : class
{
    public LambdaExpression NavigationLambdaExpression => Navigation;
    public Expression<Func<TEntity, TProperty?>> Navigation { get; protected set; } = navigation;
    public IThenQueryInclude<TPropertyUnderlying>? ThenIncludes { get; protected set; } = thenIncludes;
   
    public ThenQueryIncludeList<TPropertyUnderlying, TNextProperty> ThenIncludeList<TNextProperty>(
        Expression<Func<TPropertyUnderlying, IEnumerable<TNextProperty>?>> navigation,
        Expression<Func<TNextProperty, bool>>? filter = null) where TNextProperty : class
    {
        var thenQueryInclude = new ThenQueryIncludeList<TPropertyUnderlying, TNextProperty>(navigation, filter, null);
        ThenIncludes = thenQueryInclude;
        return thenQueryInclude;
    }

    public ThenQueryIncludeEntity<TPropertyUnderlying, TNextProperty> ThenIncludeEntity<TNextProperty>(
        Expression<Func<TPropertyUnderlying, TNextProperty?>> navigation) where TNextProperty : class
    {
        var propertyType = typeof(TNextProperty);
        if (typeof(IEnumerable).IsAssignableFrom(propertyType))
        {
            throw new ArgumentException("Use IncludeList for collections");
        }

        var thenQueryInclude = new ThenQueryIncludeEntity<TPropertyUnderlying, TNextProperty>(navigation, null);
        ThenIncludes = thenQueryInclude;
        return thenQueryInclude;
    }
    
    public Type GetPropertyType()
    {
        return typeof(TPropertyUnderlying);
    }

    public IQueryInclude? GetThenInclude()
    {
        if (ThenIncludes is IQueryInclude queryInclude)
        {
            return queryInclude;
        }

        return null;
    }
}