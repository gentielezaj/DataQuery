// Ignore Spelling: Queryable

using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryInfo.Models.IncludeModels;

public interface IIncludeInfo<TEntity>
{
    IQueryable<TEntity> ToQueryable(IQueryable<TEntity> source);
}

public abstract class CoreIncludeInfo<TEntity, TProperty, TPropertyCore>(
    Expression<Func<TEntity, TProperty?>> navigation,
    IThenIncludeInfo<TPropertyCore>? thenIncludes = null)
    where TEntity : class
    where TPropertyCore : class
{
    protected LambdaExpression NavigationLambdaExpression => Navigation;
    protected Expression<Func<TEntity, TProperty?>> Navigation { get; set; } = navigation;
    protected IThenIncludeInfo<TPropertyCore>? ThenIncludes { get; set; } = thenIncludes;

    public ThenIncludeInfoList<TPropertyCore, TNextProperty> ThenIncludeList<TNextProperty>(
        Expression<Func<TPropertyCore, IEnumerable<TNextProperty>?>> navigation,
        Func<TNextProperty, bool>? filter = null) where TNextProperty : class
    {
        var thenIncludeInfo = new ThenIncludeInfoList<TPropertyCore, TNextProperty>(navigation, filter, null);
        ThenIncludes = thenIncludeInfo;
        return thenIncludeInfo;
    }

    public ThenIncludeInfoEntity<TPropertyCore, TNextProperty> ThenIncludeEntity<TNextProperty>(
        Expression<Func<TPropertyCore, TNextProperty?>> navigation) where TNextProperty : class
    {
        var propertyType = typeof(TNextProperty);
        if (typeof(IEnumerable).IsAssignableFrom(propertyType))
        {
            throw new ArgumentException("Use IncludeList for collections");
        }

        var thenIncludeInfo = new ThenIncludeInfoEntity<TPropertyCore, TNextProperty>(navigation, null);
        ThenIncludes = thenIncludeInfo;
        return thenIncludeInfo;
    }

    protected PropertyInfo GetPropertyInfo()
    {
        PropertyInfo? propertyInfo = null;
        if (NavigationLambdaExpression.Body is MemberExpression memberExpr)
        {
            propertyInfo = memberExpr.Member as PropertyInfo;
        }
        // Handles conversions, e.g., object casting
        if (NavigationLambdaExpression.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression member)
        {
            propertyInfo = member.Member as PropertyInfo;
        }

        if (propertyInfo is null) 
            throw new ArgumentException("Property selector dose not point to a property");

        return propertyInfo;
    }
}
