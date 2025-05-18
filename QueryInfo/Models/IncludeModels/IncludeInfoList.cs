// Ignore Spelling: Queryable

using Microsoft.EntityFrameworkCore;
using QueryInfo.Resolvers;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryInfo.Models.IncludeModels;

public class IncludeInfoList<T, TProperty>(
    Expression<Func<T, IEnumerable<TProperty>?>> navigation,
    Func<TProperty, bool>? filter,
    IThenIncludeInfo<TProperty>? thenIncludes)
    : CoreIncludeInfo<T, IEnumerable<TProperty>, TProperty>(navigation, thenIncludes), IIncludeInfo<T>
    where T : class
    where TProperty : class
{
    private Func<TProperty, bool>? Filter { get; set; } = filter;
    public int? Take { get; set; }

    public int? Skip { get; set; }

    public List<OrderInfo>? Order { get; set; }

    public IQueryable<T> ToQueryable(IQueryable<T> source)
    {
        if (Filter is null && !Take.HasValue && !Skip.HasValue)
        {
            return source.Include(Navigation);
        }

        return source.Include(Navigation);

        // For now EF core dose not support include filter as variable, so we need to do it manually

        //var rootType = typeof(T);

        //PropertyInfo property = GetPropertyInfo();
        //var parameter = Expression.Parameter(rootType, "x");
        //Expression selector = Expression.PropertyOrField(parameter, property.Name);

        //var paramType = selector.Type.GetGenericArguments()[0];
        //if (Order?.Any() == true)
        //{
        //    selector = IncludeQuerableResolver.OrderBy(selector, paramType, Order.ToArray());
        //}
        //if (Skip.GetValueOrDefault(0) > 0)
        //{
        //    var method = typeof(Enumerable).GetMethod("Skip")!.MakeGenericMethod(paramType);
        //    selector = Expression.Call(method, selector, Expression.Constant(Skip.Value));
        //}
        //if (Take.GetValueOrDefault(0) > 0)
        //{
        //    var ms = typeof(Enumerable).GetMethods();
        //    var msTake = ms.Where(x => x.Name == "Take");
        //    var method = typeof(Enumerable).GetMethod("Take", new[] { typeof(System.Collections.IEnumerable), typeof(int) });
        //    method ??= msTake.First();
        //    method = method.MakeGenericMethod(paramType);
        //    selector = Expression.Call(method, selector, Expression.Constant(Take.Value));
        //}
        //if (item.QueryInfo.GetExpression() is Expression queryInfoWhere && queryInfoWhere is not null)
        //{
        //    var method = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == "Where")!.MakeGenericMethod(paramType);
        //    selector = Expression.Call(method, selector, queryInfoWhere);
        //}

        //Expression quote = Expression.Quote(Expression.Lambda(selector, parameter));
        //expression = Expression.Call(typeof(EntityFrameworkQueryableExtensions), "Include",
        //        new[] { source.ElementType, selector.Type },
        //        expression, quote);

        //if (property.PropertyType.FullName!.StartsWith("System.Collections.Generic"))
        //{
        //    rootType = selector.Type.GetGenericArguments()[0];
        //}
        //else
        //{
        //    rootType = property.PropertyType;
        //}

        //return ThenIncludes is null
        //    ? include
        //    : ThenIncludes.ToQueryableList(include!);
    }
}
