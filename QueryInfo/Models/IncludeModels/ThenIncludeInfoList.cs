// Ignore Spelling: Queryable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace QueryInfo.Models.IncludeModels;

public class ThenIncludeInfoList<TPreviousProperty, TNextProperty>(
    Expression<Func<TPreviousProperty, IEnumerable<TNextProperty>?>> navigation,
    Expression<Func<TNextProperty, bool>>? filter,
    IThenIncludeInfo<TNextProperty>? thenIncludes)
    : CoreIncludeInfo<TPreviousProperty, IEnumerable<TNextProperty>, TNextProperty>(navigation, thenIncludes),
        IThenIncludeInfo<TPreviousProperty>
    where TPreviousProperty : class
    where TNextProperty : class
{
    private Expression<Func<TNextProperty, bool>>? Filter { get; set; } = filter;
    public int? Take { get; set; }
    public int? Skip { get; set; }
    public IOrderInfo<TNextProperty>? OrderInfo { get; set; }

    public IQueryable<TSource> ToQueryableEntity<TSource>(IIncludableQueryable<TSource, TPreviousProperty> source)
        where TSource : class
    {
        if (filter is null)
        {
            var included = source.ThenInclude(Navigation);
            
            return ThenIncludes is null
                ? included
                : ThenIncludes.ToQueryableList(included!);
        }

        // not supported yet
        // if (Filter != null)
        // {
        //     // Apply filter to the included collection
        //     included = included.ThenInclude(e => e!.Where(Filter.Compile()));
        // }

        return ToQueryable(source);
    }

    public IQueryable<TSource> ToQueryableList<TSource>(
        IIncludableQueryable<TSource, IEnumerable<TPreviousProperty>> source) where TSource : class
    {
        if (filter is null)
        {
            var included = source.ThenInclude(Navigation);
            
            return ThenIncludes is null
                ? included
                : ThenIncludes.ToQueryableList(included!);
        }

        // not supported yet
        // if (Filter != null)
        // {
        //     // Apply filter to the included collection
        //     included = included.ThenInclude(e => e!.Where(Filter));
        // }
        
        return ToQueryable(source);
    }

    public IQueryable<TSource> ToQueryable<TSource>(IQueryable<TSource> source)
    {
        var rootType = typeof(TNextProperty);
        var expression = source.Expression;

        var property = GetPropertyInfo();
        var parameter = Expression.Parameter(rootType, "x");
        Expression selector = Expression.PropertyOrField(parameter, property.Name);

        var paramType = selector.Type.GetGenericArguments()[0];
        if (OrderInfo is not null)
        {
            var orderExpression = OrderInfo.ToExpression(selector);
            if (orderExpression is not null)
                selector = orderExpression;
        }
        if (Skip.GetValueOrDefault(0) > 0)
        {
            var method = typeof(Enumerable).GetMethod("Skip")!.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, Expression.Constant(Skip));
        }
        if (Take.GetValueOrDefault(0) > 0)
        {
            var ms = typeof(Enumerable).GetMethods();
            var msTake = ms.Where(x => x.Name == "Take");
            var method = typeof(Enumerable).GetMethod("Take", [ typeof(System.Collections.IEnumerable), typeof(int) ]);
            method ??= msTake.First();
            method = method.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, Expression.Constant(Take));
        }
        if (Filter is not null)
        {
            var method = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == "Where")!.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, Filter);
        }

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