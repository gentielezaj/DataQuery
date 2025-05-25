using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using shlabs.DataQuery.Abstractions;
using shlabs.DataQuery.Abstractions.Utilities;

namespace System.Linq;

public static class QueryIncludeToQueryable
{
    public static IQueryable<T> ToQueryable<T>(this IQueryInclude<T> queryInclude, IQueryable<T> query)
        where T : class
    {
        var entityType = typeof(T);
        var propertyType = queryInclude.GetPropertyType();
        
        if (queryInclude is IQueryIncludeEntity<T> queryIncludeEntity)
        {
            var method = GetMethod(nameof(ToQueryableEntity))
                .MakeGenericMethod(entityType, propertyType);
            
            return (method.Invoke(null, [queryIncludeEntity, query]) as IQueryable<T>)!;
        }
        
        if (queryInclude is IQueryIncludeList<T> queryIncludeList)
        {
            var method = GetMethod(nameof(ToQueryableList))
                .MakeGenericMethod(entityType, propertyType);
            
            return (method.Invoke(null, [queryIncludeList, query]) as IQueryable<T>)!;
        }
        
        throw new ArgumentException($"{nameof(queryInclude)} must be of type {nameof(IQueryIncludeEntity<T>)}");
    }
    
    private static IQueryable<T> ToQueryableEntity<T, TProperty>(QueryIncludeEntity<T, TProperty> queryInclude, IQueryable<T> query)
        where T : class
        where TProperty : class
    {
        var include = query.Include(queryInclude.Navigation);
        return queryInclude.ThenIncludes is null
            ? include
            : ThenToQueryable<T, TProperty>(queryInclude.ThenIncludes!, include);
    }
    
    private static IQueryable<T> ToQueryableList<T, TProperty>(QueryIncludeList<T, TProperty> queryInclude, IQueryable<T> source)
        where T : class
        where TProperty : class
    {
        // if (queryInclude.Filter is null && !queryInclude.Take.HasValue && !queryInclude..HasValue)
        // {
        //     var queryNavigation = source.Include(Navigation);
        //     
        //     return ThenIncludes is null
        //         ? queryNavigation
        //         : ThenIncludes.ToQueryable(queryNavigation);
        // }

        // For now EF core dose not support include filter as variable, so we need to do it manually

        var rootType = typeof(T);
        var expression = source.Expression;

        PropertyInfo property = ExpressionUtils.GetPropertyInfo(queryInclude.Navigation);
        var parameter = Expression.Parameter(rootType, "x");
        Expression selector = Expression.PropertyOrField(parameter, property.Name);

        var paramType = selector.Type.GetGenericArguments()[0];
        if (queryInclude.Order is not null)
        {
            var orderInfos = queryInclude.Order.GetOrderAsList();
            var count = 0;
            foreach (var item in orderInfos)
            {
                var orderParameter = Expression.Parameter(typeof(TProperty), "x");
                Expression orderSelector = ExpressionUtils.BuildSelector(orderParameter, item.LambdaSelector.Body);

                var methodName = Equals(item.Direction, QueryOrderDirections.Desc)
                    ? (count == 0 ? "OrderByDescending" : "ThenByDescending")
                    : (count == 0 ? "OrderBy" : "ThenBy");

                selector = Expression.Call(typeof(Enumerable), methodName,
                    [paramType, orderSelector.Type],
                    selector, Expression.Lambda(orderSelector, orderParameter));
                count++;
            }

            // var orderExpression = QueryOrder.ToExpression(selector);
            // if (orderExpression is not null)
            //     selector = orderExpression;
        }
        if (queryInclude.Skip.GetValueOrDefault(0) > 0)
        {
            var method = typeof(Enumerable).GetMethod("Skip")!.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, Expression.Constant(queryInclude.Skip));
        }
        if (queryInclude.Take.GetValueOrDefault(0) > 0)
        {
            var ms = typeof(Enumerable).GetMethods();
            var msTake = ms.Where(x => x.Name == "Take");
            var method = typeof(Enumerable).GetMethod("Take", new[] { typeof(System.Collections.IEnumerable), typeof(int) });
            method ??= msTake.First();
            method = method.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, Expression.Constant(queryInclude.Take));
        }
        if (queryInclude.Filter is not null)
        {
            var method = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == "Where")!.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, queryInclude.Filter);
        }

        Expression quote = Expression.Quote(Expression.Lambda(selector, parameter));
        expression = Expression.Call(typeof(EntityFrameworkQueryableExtensions), "Include",
                new[] { source.ElementType, selector.Type },
                expression, quote);

        var query = source.Provider.CreateQuery<T>(expression);

        return queryInclude.ThenIncludes is null
            ? query
            : ThenToQueryable(queryInclude.ThenIncludes!, query);
    }
    
    private static IQueryable<TSource> ThenToQueryable<TSource, TProperty>(this IThenQueryInclude<TProperty> queryInclude, IQueryable<TSource> query) 
        where TProperty : 
        class where TSource : class
    {
        var nextType = queryInclude.GetNextPropertyType();
        if (queryInclude is IThenQueryIncludeEntity<TProperty> queryIncludeEntity)
        {
            var method = GetMethod(nameof(ThenToQueryableEntity))
                .MakeGenericMethod(typeof(TSource), typeof(TProperty), nextType);
            
            return (method.Invoke(null, [queryIncludeEntity, query]) as IQueryable<TSource>)!;
        }
        
        if (queryInclude is IThenQueryIncludeList<TProperty> queryIncludeList)
        {
            var method = GetMethod(nameof(ThenToQueryableList))
                .MakeGenericMethod(typeof(TSource), typeof(TProperty), nextType);
            
            return (method.Invoke(null, [queryIncludeList, query]) as IQueryable<TSource>)!;
        }
        
        throw new ArgumentException($"{nameof(queryInclude)} must be of type {nameof(IQueryIncludeEntity<TSource>)}");
    }

    private static IQueryable<TSource> ThenToQueryableEntity<TSource, TPreviousProperty, TNextProperty>(ThenQueryIncludeEntity<TPreviousProperty, TNextProperty> queryInclude,
        IQueryable<TSource> source)
        where TSource : class
        where TNextProperty : class
        where TPreviousProperty : class
    {
        var rootType = typeof(TPreviousProperty);
        var expression = source.Expression;

        var property = ExpressionUtils.GetPropertyInfo(queryInclude.Navigation);
        var parameter = Expression.Parameter(rootType, "x");
        Expression selector = Expression.PropertyOrField(parameter, property.Name);

        Expression quote = Expression.Quote(Expression.Lambda(selector, parameter));
        expression = Expression.Call(typeof(EntityFrameworkQueryableExtensions), "ThenInclude",
            new[] { source.ElementType, rootType, selector.Type },
            expression, quote);

        var query = source.Provider.CreateQuery<TSource>(expression);

        return queryInclude.ThenIncludes is null
            ? query
            : ThenToQueryable(queryInclude.ThenIncludes!, query);
    }
    
    private static IQueryable<TSource> ThenToQueryableList<TSource, TPreviousProperty, TNextProperty>(ThenQueryIncludeList<TPreviousProperty, TNextProperty> queryInclude,
        IQueryable<TSource> source)
        where TSource : class
        where TNextProperty : class
        where TPreviousProperty : class
    {
        var rootType = typeof(TPreviousProperty);
        var expression = source.Expression;

        var property = ExpressionUtils.GetPropertyInfo(queryInclude.Navigation);
        var parameter = Expression.Parameter(rootType, "x");
        Expression selector = Expression.PropertyOrField(parameter, property.Name);

        var paramType = selector.Type.GetGenericArguments()[0];
        if (queryInclude.Order is not null)
        {
            var orderInfos = queryInclude.Order.GetOrderAsList();
            var count = 0;
            foreach (var item in orderInfos)
            {
                var orderParameter = Expression.Parameter(typeof(TPreviousProperty), "x");
                Expression orderSelector = ExpressionUtils.BuildSelector(orderParameter, item.LambdaSelector.Body);

                var methodName = Equals(item.Direction, QueryOrderDirections.Desc)
                    ? (count == 0 ? "OrderByDescending" : "ThenByDescending")
                    : (count == 0 ? "OrderBy" : "ThenBy");

                selector = Expression.Call(typeof(Enumerable), methodName,
                    [paramType, orderSelector.Type],
                    selector, Expression.Lambda(orderSelector, orderParameter));
                count++;
            }
        }
        if (queryInclude.Skip.GetValueOrDefault(0) > 0)
        {
            var method = typeof(Enumerable).GetMethod("Skip")!.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, Expression.Constant(queryInclude.Skip));
        }
        if (queryInclude.Take.GetValueOrDefault(0) > 0)
        {
            var ms = typeof(Enumerable).GetMethods();
            var msTake = ms.Where(x => x.Name == "Take");
            var method = typeof(Enumerable).GetMethod("Take", [ typeof(System.Collections.IEnumerable), typeof(int) ]);
            method ??= msTake.First();
            method = method.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, Expression.Constant(queryInclude.Take));
        }
        if (queryInclude.Filter is not null)
        {
            var method = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == "Where")!.MakeGenericMethod(paramType);
            selector = Expression.Call(method, selector, queryInclude.Filter);
        }

        Expression quote = Expression.Quote(Expression.Lambda(selector, parameter));
        expression = Expression.Call(typeof(EntityFrameworkQueryableExtensions), "ThenInclude",
            new[] { source.ElementType, rootType, selector.Type },
            expression, quote);

        var query = source.Provider.CreateQuery<TSource>(expression);

        return queryInclude.ThenIncludes is null
            ? query
            : ThenToQueryable(queryInclude.ThenIncludes!, query);
    }

    private static MethodInfo GetMethod(string name)
    {
        return typeof(QueryIncludeToQueryable).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;
    }
    
}