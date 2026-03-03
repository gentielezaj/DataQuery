using shlabs.DataQuery.Abstractions.Utils;
using System.Collections;
using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions;

public abstract class QueryInclude<TEntity, TProperty, TPropertyUnderlying>(
    Expression<Func<TEntity, TProperty?>> navigation,
    IThenQueryInclude<TPropertyUnderlying>? thenIncludes = null) 
    : QueryInclude<TEntity, TPropertyUnderlying>(thenIncludes), IQueryInclude<TEntity>
    where TEntity : class
    where TPropertyUnderlying : class
{
    public override LambdaExpression NavigationLambdaExpression => Navigation;
    public Expression<Func<TEntity, TProperty?>> Navigation { get; set; } = navigation;
}

public abstract class QueryInclude<TEntity, TPropertyUnderlying>(
    IThenQueryInclude<TPropertyUnderlying>? thenIncludes = null) : IQueryInclude<TEntity>
    where TPropertyUnderlying : class
    where TEntity : class
{
    public abstract LambdaExpression NavigationLambdaExpression { get; }
    public IThenQueryInclude<TPropertyUnderlying>? ThenIncludes { get; set; } = thenIncludes;

    public ThenQueryIncludeList<TPropertyUnderlying, TNextProperty> ThenInclude<TNextProperty>(
        Expression<Func<TPropertyUnderlying, IEnumerable<TNextProperty>?>> navigation,
        Expression<Func<TNextProperty, bool>>? filter = null) where TNextProperty : class
    {
        return ThenIncludeList(navigation, filter);
    }

    public ThenQueryIncludeList<TPropertyUnderlying, TNextProperty> ThenInclude<TNextProperty>(
        Expression<Func<TPropertyUnderlying, ICollection<TNextProperty>?>> navigation,
        Expression<Func<TNextProperty, bool>>? filter = null) where TNextProperty : class
    {
        return ThenIncludeList(navigation.ToEnumerableExpression(), filter);
    }

    public ThenQueryIncludeEntity<TPropertyUnderlying, TNextProperty> ThenInclude<TNextProperty>(
        Expression<Func<TPropertyUnderlying, TNextProperty?>> navigation) where TNextProperty : class
    {
        return ThenIncludeEntity(navigation);
    }

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