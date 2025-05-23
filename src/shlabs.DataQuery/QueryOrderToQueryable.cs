using System.Linq.Expressions;
using shabs.DataQuery.Abstractions;
using shabs.DataQuery.Abstractions.Utilities;

namespace System.Linq;

public static class QueryOrderToQueryable
{
    public static IQueryable<T> ToQueryable<T>(this IQueryOrder<T> queryOrder, IQueryable<T> query)
    {
        var expression = query.Expression;
        var orderInfos = queryOrder.GetOrderAsList();
        var count = 0;
        foreach (var item in orderInfos)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression selector = ExpressionUtils.BuildSelector(parameter, queryOrder.LambdaSelector.Body);

            var methodName = Equals(item.Direction, QueryOrderDirections.Desc)
                ? (count == 0 ? "OrderByDescending" : "ThenByDescending")
                : (count == 0 ? "OrderBy" : "ThenBy");

            expression = Expression.Call(typeof(Queryable), methodName,
                [typeof(T), selector.Type],
                expression, Expression.Quote(Expression.Lambda(selector, parameter)));
            count++;
        }

        if (count == 0)
        {
            return query;
        }
        
        return query.Provider.CreateQuery<T>(expression);
    }
}