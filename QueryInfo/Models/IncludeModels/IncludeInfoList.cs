// Ignore Spelling: Queryable

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryInfo.Models.IncludeModels;

public class IncludeInfoList<T, TProperty>(
    Expression<Func<T, IEnumerable<TProperty>?>> navigation,
    Expression<Func<TProperty, bool>>? filter,
    IThenIncludeInfo<TProperty>? thenIncludes)
    : CoreIncludeInfo<T, IEnumerable<TProperty>, TProperty>(navigation, thenIncludes), IIncludeInfo<T>
    where T : class
    where TProperty : class
{
    private Expression<Func<TProperty, bool>>? Filter { get; set; } = filter;
    public int? Take { get; set; }
    public int? Skip { get; set; }
    public IOrderInfo<T>? OrderInfo { get; set; }

    public IQueryable<T> ToQueryable(IQueryable<T> source)
    {
        if (Filter is null && !Take.HasValue && !Skip.HasValue)
        {
            return source.Include(Navigation);
        }

        // For now EF core dose not support include filter as variable, so we need to do it manually

        var rootType = typeof(T);
        var expression = source.Expression;

        PropertyInfo property = GetPropertyInfo();
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
            selector = Expression.Call(method, selector, Expression.Constant(Skip.Value));
        }
        if (Take.GetValueOrDefault(0) > 0)
        {
            var ms = typeof(Enumerable).GetMethods();
            var msTake = ms.Where(x => x.Name == "Take");
            var method = typeof(Enumerable).GetMethod("Take", new[] { typeof(System.Collections.IEnumerable), typeof(int) });
            method ??= msTake.First();
            method = method.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, Expression.Constant(Take.Value));
        }
        if (Filter is not null)
        {
            var method = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == "Where")!.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, Filter);
        }

        Expression quote = Expression.Quote(Expression.Lambda(selector, parameter));
        expression = Expression.Call(typeof(EntityFrameworkQueryableExtensions), "Include",
                new[] { source.ElementType, selector.Type },
                expression, quote);

        var query = source.Provider.CreateQuery<T>(expression);

        return ThenIncludes is null
            ? query
            : ThenIncludes.ToQueryable(query!);
    }
}
