using System.Linq.Expressions;

namespace shabs.DataQuery.Abstractions;

public interface IThenQueryIncludeList<TPreviousProperty> : IThenQueryInclude<TPreviousProperty>
    where TPreviousProperty : class;

public class ThenQueryIncludeList<TPreviousProperty, TNextProperty>(
    Expression<Func<TPreviousProperty, IEnumerable<TNextProperty>?>> navigation,
    Expression<Func<TNextProperty, bool>>? where,
    IThenQueryInclude<TNextProperty>? thenIncludes)
    : CoreQueryIncludeList<TPreviousProperty, TNextProperty>(navigation, where, thenIncludes),
        IThenQueryIncludeList<TPreviousProperty>
    where TPreviousProperty : class
    where TNextProperty : class
{
    public Type GetNextPropertyType()
    {
        return typeof(TNextProperty);
    }
}