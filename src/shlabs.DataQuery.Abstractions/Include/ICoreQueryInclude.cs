using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions;

public interface IQueryInclude
{
    LambdaExpression NavigationLambdaExpression { get; }
    Type GetPropertyType();
    IQueryInclude? GetThenInclude(); 
}