using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions;

public interface IThenQueryIncludeEntity<TPreviousProperty> : IThenQueryInclude<TPreviousProperty>
    where TPreviousProperty : class;

public class ThenQueryIncludeEntity<TPreviousProperty, TNextProperty>(
    Expression<Func<TPreviousProperty, TNextProperty?>> navigation,
    IThenQueryInclude<TNextProperty>? thenIncludes)
    : CoreQueryInclude<TPreviousProperty, TNextProperty, TNextProperty>(navigation, thenIncludes),
        IThenQueryIncludeEntity<TPreviousProperty>
    where TPreviousProperty : class
    where TNextProperty : class
{
    public Type GetNextPropertyType()
    {
        return typeof(TNextProperty);
    }
}