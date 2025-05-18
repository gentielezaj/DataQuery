// Ignore Spelling: Queryable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace QueryInfo.Models.IncludeModels;

public class ThenIncludeInfoList<TPreviousProperty, TNextProperty>(
    Expression<Func<TPreviousProperty, IEnumerable<TNextProperty>?>> navigation,
    Func<TNextProperty, bool>? filter,
    IThenIncludeInfo<TNextProperty>? thenIncludes)
    : CoreIncludeInfo<TPreviousProperty, IEnumerable<TNextProperty>, TNextProperty>(navigation, thenIncludes),
        IThenIncludeInfo<TPreviousProperty>
    where TPreviousProperty : class
    where TNextProperty : class
{
    private Func<TNextProperty, bool>? Filter { get; set; } = filter;

    public IQueryable<TSource> ToQueryableEntity<TSource>(IIncludableQueryable<TSource, TPreviousProperty> source)
        where TSource : class
    {
        var included = source.ThenInclude(Navigation);

        if (Filter != null)
        {
            // Apply filter to the included collection
            included = included.ThenInclude(e => e!.Where(Filter));
        }

        return ThenIncludes is null
            ? included
            : ThenIncludes.ToQueryableList(included!);
    }

    public IQueryable<TSource> ToQueryableList<TSource>(
        IIncludableQueryable<TSource, IEnumerable<TPreviousProperty>> source) where TSource : class
    {
        var included = source.ThenInclude(Navigation);

        if (Filter != null)
        {
            // Apply filter to the included collection
            included = included.ThenInclude(e => e!.Where(Filter));
        }

        return ThenIncludes is null
            ? included
            : ThenIncludes.ToQueryableList(included!);
    }
}