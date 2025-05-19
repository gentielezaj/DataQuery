// Ignore Spelling: Queryable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace QueryInfo.Models.IncludeModels;

public class ThenIncludeInfoEntity<TPreviousProperty, TNextProperty>(
    Expression<Func<TPreviousProperty, TNextProperty?>> navigation,
    IThenIncludeInfo<TNextProperty>? thenIncludes)
    : CoreIncludeInfo<TPreviousProperty, TNextProperty, TNextProperty>(navigation, thenIncludes),
        IThenIncludeInfo<TPreviousProperty>
    where TPreviousProperty : class
    where TNextProperty : class
{
    public IQueryable<TSource> ToQueryableEntity<TSource>(
        IIncludableQueryable<TSource, TPreviousProperty> source)
        where TSource : class
    {
        var included = source.ThenInclude(Navigation);
        return thenIncludes is null
            ? included
            : thenIncludes.ToQueryableEntity(included!);
    }

    public IQueryable<TSource> ToQueryableList<TSource>(
        IIncludableQueryable<TSource, IEnumerable<TPreviousProperty>> source)
        where TSource : class
    {
        var included = source.ThenInclude(Navigation);
        return thenIncludes is null
            ? included
            : thenIncludes.ToQueryableEntity(included!);
    }

    public IQueryable<TSource> ToQueryable<TSource>(IQueryable<TSource> source)
    {
        var rootType = typeof(TNextProperty);
        var expression = source.Expression;

        var property = GetPropertyInfo();
        var parameter = Expression.Parameter(rootType, "x");
        Expression selector = Expression.PropertyOrField(parameter, property.Name);

        Expression quote = Expression.Quote(Expression.Lambda(selector, parameter));
        expression = Expression.Call(typeof(EntityFrameworkQueryableExtensions), "ThenInclude",
            new[] { source.ElementType, rootType, selector.Type },
            expression, quote);

        var query = source.Provider.CreateQuery<TSource>(expression);

        return ThenIncludes is null
            ? query
            : ThenIncludes.ToQueryable(query);
    }
}
