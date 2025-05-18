using Microsoft.EntityFrameworkCore;
using QueryInfo.Models;
using QueryInfo.Models.IncludeModels;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryInfo.Resolvers;
internal class IncludeQuerableResolver
{
    //public static Expression OrderBy(Expression expression, Type type, OrderInfo[] orderInfos)
    //{
    //    if (orderInfos.Any() != true)
    //    {
    //        return expression;
    //    }

    //    var count = 0;
    //    foreach (var item in orderInfos)
    //    {
    //        var parameter = Expression.Parameter(type, "x");
    //        Expression selector = ResolveQueryOrder(item.Property, parameter, type);

    //        var methodName = Equals(item.OrderType, OrderInfoDirections.Desc) ?
    //            (count == 0 ? "OrderByDescending" : "ThenByDescending") :
    //            (count == 0 ? "OrderBy" : "ThenBy");

    //        var method = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == methodName)!
    //            .MakeGenericMethod(type, selector.Type);

    //        expression = Expression.Call(method, expression, Expression.Lambda(selector, parameter));
    //        count++;
    //    }
    //    return expression;
    //}

    //private static Expression ResolveQueryOrder(QueryInfoOrderPropertyDefinition propertyDefinition, Expression selector, Type parentType)
    //{
    //    if (propertyDefinition is QueryInfoOrderPropertyFunctiondDefinition functionDefinition)
    //    {
    //        return ResolveQueryOrder(functionDefinition, selector, parentType);
    //    }

    //    selector = Expression.PropertyOrField(selector, propertyDefinition.PropertyName);
    //    return propertyDefinition.Child is null ? selector : ResolveQueryOrder(propertyDefinition.Child, selector, parentType.GetProperty(propertyDefinition.PropertyName)!.PropertyType);
    //}

    //private static Expression ResolveQueryOrder(QueryInfoOrderPropertyFunctiondDefinition propertyDefinition, Expression selector, Type parentType)
    //{
    //    Expression? selectorChild = null;
    //    parentType = parentType.GetGenericArguments().Any() ? parentType.GetGenericArguments()[0] : parentType;
    //    PropertyInfo? childProperty = null;
    //    var parameter = Expression.Parameter(parentType, "xy");
    //    if (propertyDefinition.Child != null)
    //    {
    //        childProperty = parentType.GetProperty(propertyDefinition.Child!.PropertyName)!;
    //        selectorChild = ResolveQueryOrder(propertyDefinition.Child, parameter, parentType);
    //    }

    //    if (selectorChild is not null)
    //    {
    //        var lastChildType = propertyDefinition.Child!.GetLastChildType(childProperty?.PropertyType ?? typeof(int));
    //        if (lastChildType.IsGenericType)
    //        {
    //            lastChildType = typeof(int);
    //        }
    //        var methods = typeof(Enumerable).GetMethods()
    //            .Where(m => m.Name == propertyDefinition.PropertyName && m.GetParameters().Length == 2)
    //            .First(x =>
    //            {
    //                var parameters = x.GetParameters();
    //                return parameters.Length == 2 && parameters[1].ParameterType.GetGenericArguments()[1] == lastChildType;
    //            }).MakeGenericMethod(parentType);

    //        var lambda = Expression.Lambda(selectorChild, parameter);
    //        return Expression.Call(null, methods, selector, lambda);
    //    }

    //    // TODO: define when no arguments
    //    selector = Expression.PropertyOrField(selector, propertyDefinition.PropertyName);
    //    return selector;
    //}
}

