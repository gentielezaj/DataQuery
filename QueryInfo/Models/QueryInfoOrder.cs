using System.Diagnostics.CodeAnalysis;
using QueryInfo.Utilities;
using System.Linq.Expressions;

namespace QueryInfo.Models;

public interface IOrderInfo<TEntity>
{
    LambdaExpression LambdaSelector { get; }

    OrderInfoDirections Direction { get; }

    IOrderInfo<TEntity>? ThenOrderBy { get; }

    IOrderInfo<TEntity> ThenBy(IOrderInfo<TEntity> thenBy);

    OrderInfo<TEntity, TNextProperty> ThenByAsc<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector);

    OrderInfo<TEntity, TNextProperty> ThenByDesc<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector);

    OrderInfo<TEntity, TNextProperty> ThenBy<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector,
        OrderInfoDirections direction);

    IOrderInfo<TEntity> AddOrderBy(IOrderInfo<TEntity>? thenBy);
    IOrderInfo<TEntity> AddOrderBy(IEnumerable<IOrderInfo<TEntity>>? thenBy);

    IQueryable<TEntity> ToQueryable(IQueryable<TEntity> source);

    Expression? ToExpression(Expression expression);

    IEnumerable<IOrderInfo<TEntity>> GetOrderAsList();
}

public class OrderInfo<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> selector, OrderInfoDirections direction)
    : IOrderInfo<TEntity>
{
    public Expression<Func<TEntity, TProperty>> Selector { get; init; } = selector;

    public LambdaExpression LambdaSelector => Selector;

    public OrderInfoDirections Direction { get; init; } = direction;
    public IOrderInfo<TEntity>? ThenOrderBy { get; private set; } = null;

    public OrderInfo<TEntity, TNextProperty> ThenBy<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector,
        OrderInfoDirections direction)
    {
        var orderInfo = new OrderInfo<TEntity, TNextProperty>(selector, direction);
        ThenOrderBy = orderInfo;
        return orderInfo;
    }

    public IOrderInfo<TEntity> ThenBy(IOrderInfo<TEntity> thenBy)
    {
        ThenOrderBy = thenBy;
        return ThenOrderBy;
    }

    public OrderInfo<TEntity, TNextProperty> ThenByAsc<TNextProperty>(Expression<Func<TEntity, TNextProperty>> selector)
    {
        return ThenBy(selector, OrderInfoDirections.Asc);
    }

    public OrderInfo<TEntity, TNextProperty> ThenByDesc<TNextProperty>(
        Expression<Func<TEntity, TNextProperty>> selector)
    {
        return ThenBy(selector, OrderInfoDirections.Desc);
    }

    public IOrderInfo<TEntity> AddOrderBy(IOrderInfo<TEntity>? thenBy)
        => AddOrderBy(thenBy is null ? null : [thenBy]);

    public IOrderInfo<TEntity> AddOrderBy(IEnumerable<IOrderInfo<TEntity>>? thenBy)
    {
        if (thenBy?.Any() != true)
        {
            return this;
        }

        if (ThenOrderBy is not null)
        {
            return ThenOrderBy.AddOrderBy(thenBy);
        }

        var q = new Queue<IOrderInfo<TEntity>>(thenBy);
        ThenOrderBy = q.Dequeue();
        return ThenOrderBy.AddOrderBy(q.ToArray());
    }

    public IQueryable<TEntity> ToQueryable(IQueryable<TEntity> source)
    {
        var expression = ToExpression(source.Expression);

        return expression is not null
            ? source.Provider.CreateQuery<TEntity>(expression)
            : source;
    }

    public Expression? ToExpression(Expression expression)
    {
        var orderInfos = GetOrderAsList();
        var count = 0;
        foreach (var item in orderInfos)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            Expression selector = ExpressionUtils.BuildSelector(parameter, LambdaSelector.Body);

            var methodName = Equals(item.Direction, OrderInfoDirections.Desc)
                ? (count == 0 ? "OrderByDescending" : "ThenByDescending")
                : (count == 0 ? "OrderBy" : "ThenBy");

            expression = Expression.Call(typeof(Queryable), methodName,
                [typeof(TEntity), selector.Type],
                expression, Expression.Quote(Expression.Lambda(selector, parameter)));
            count++;
        }

        return count > 0 ? expression : null;
    }

    public IEnumerable<IOrderInfo<TEntity>> GetOrderAsList()
    {
        var list = new List<IOrderInfo<TEntity>> { this };
        if (ThenOrderBy is not null)
        {
            list.AddRange(ThenOrderBy.GetOrderAsList());
        }

        return list;
    }

    public IQueryable<TEntity> ToQueryableNotSupported(IQueryable<TEntity> source)
    {
        IOrderedQueryable<TEntity>? orderedQueryable = null;
        if (source is IOrderedQueryable<TEntity> order)
        {
            orderedQueryable = Direction == OrderInfoDirections.Asc
                ? order.ThenBy(Selector)
                : order.ThenByDescending(Selector);
        }
        else
        {
            orderedQueryable = Direction == OrderInfoDirections.Asc
                ? source.OrderBy(Selector)
                : source.OrderByDescending(Selector);
        }

        if (ThenOrderBy is not null)
        {
            return ThenOrderBy.ToQueryable(orderedQueryable);
        }

        return orderedQueryable;
    }

    private static Expression GetSecector(Expression paramerter, LambdaExpression selector)
    {
        Expression? expr = selector.Body;

        return BuildSelector(paramerter, expr);
        
        // if (expr is MemberExpression memberExpr)
        // {
        //    return GetSecector(paramerter, memberExpr); 
        // }
        //
        // if (expr is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression member)
        // {
        //     return GetSecector(paramerter, member); 
        // }
        //
        // if (expr is MethodCallExpression methodCallExpr)
        // {
        //     return GetSecectorMethod(paramerter, methodCallExpr);
        // }
        //
        // return expr;
    }
    
    public static Expression BuildSelector(Expression parameter, Expression body)
    {
        if (body is MemberExpression memberExpr)
        {
            var inner = BuildSelector(parameter, memberExpr.Expression ?? parameter);
            return Expression.PropertyOrField(inner, memberExpr.Member.Name);
        }
        if (body is MethodCallExpression methodCall)
        {
            var args = new List<Expression>();

            foreach (var arg in methodCall.Arguments)
            {
                if (arg is LambdaExpression lambda)
                {
                    var inner = BuildSelector(parameter, lambda.Body);
                    var lambdaExpression = Expression.Lambda(inner, lambda.Parameters);
                    var quoteExpression = Expression.Quote(lambdaExpression);
                    args.Add(quoteExpression);
                }
                else
                {
                    var innerBody = BuildSelector(parameter, arg);
                    args.Add(innerBody);
                }
            }
            
            var method = methodCall.Method.IsGenericMethod
                ? methodCall.Method.GetGenericMethodDefinition().MakeGenericMethod(methodCall.Method.GetGenericArguments())
                : methodCall.Method;

            var instance = methodCall.Object != null ? BuildSelector(parameter, methodCall.Object) : null;
            return Expression.Call(instance, method, args);
        }
        if (body is ParameterExpression)
        {
            return parameter;
        }
        if (body is UnaryExpression unary)
        {
            return BuildSelector(parameter, unary.Operand);
        }
        throw new NotSupportedException($"Unsupported expression type: {body.GetType().Name}");
    }
    
    private static Expression GetSecector(Expression paramerter, MemberExpression selector)
    {
        if (selector.Expression is MemberExpression memberExpr)
        {
            paramerter = GetSecector(paramerter, memberExpr);
        }
        
        return Expression.PropertyOrField(paramerter, selector.Member.Name);
    }

    public static Expression GetSecectorMethod(Expression parameter, MethodCallExpression methodCallExpr)
    {
        var parentType = methodCallExpr.Method.GetGenericArguments()[0];
        parentType = parentType.GetGenericArguments().Any() ? parentType.GetGenericArguments()[0] : parentType;

        var methods = typeof(Enumerable).GetMethods()
            .Where(m => m.Name == methodCallExpr.Method.Name &&
                        m.GetParameters().Length == methodCallExpr.Method.GetParameters().Length
                        && m.GetGenericArguments().Length == methodCallExpr.Method.GetGenericArguments().Length);
        
        var method = methods
            .First(x =>
            {
                if (x.IsGenericMethod)
                {
                    x = x.MakeGenericMethod(methodCallExpr.Method.GetGenericArguments());
                }
                
                var parameters = x.GetParameters();
                var callerParameters = methodCallExpr.Method.GetParameters();

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != callerParameters[i].ParameterType 
                        && !callerParameters[i].ParameterType.IsAssignableFrom(parameters[i].ParameterType))
                    {
                        return false;
                    }
                }

                return true;
            }).MakeGenericMethod(parentType);
        
        var methodCallExprArguments = methodCallExpr.Arguments;
        if (methodCallExprArguments.Count == 1)
        {
            return Expression.Call(null, method, parameter);
        }

        if (methodCallExprArguments.Count != 2)
        {
            throw new ArgumentException("Method is not supported yet, ", nameof(method.Name));
        }
        
        var chaildParameterName = "x" + Guid.NewGuid().ToString("N").ToLowerInvariant();
        var childParameter = Expression.Parameter(parentType, chaildParameterName);
        
        var childSelector = GetSecector(childParameter, (LambdaExpression)methodCallExprArguments[1]);
        var childLambda = Expression.Lambda(childSelector, childParameter);
        return Expression.Call(null, method, parameter, Expression.Quote(childLambda));
    }
}

public class OrderInfo(string field, OrderInfoDirections direction)
{
    [Obsolete("This is only needed on controllers")]
    public OrderInfo() : this(string.Empty, OrderInfoDirections.Asc)
    {
    }

    public OrderInfoDirections Direction { get; set; } = direction;
    public string Field { get; set; } = field;

    public static OrderInfo Asc(string field, params string[] fields)
    {
        var orderField = fields.Aggregate(field, (current, f) => current + $".{f}");
        return new OrderInfo(orderField, OrderInfoDirections.Asc);
    }

    public static OrderInfo Desc(string field, params string[] fields)
    {
        var orderField = fields.Aggregate(field, (current, f) => current + $".{f}");
        return new OrderInfo(orderField, OrderInfoDirections.Desc);
    }
}

public enum OrderInfoDirections
{
    Asc,
    Desc
}