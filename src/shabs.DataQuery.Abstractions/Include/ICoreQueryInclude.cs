using System.Linq.Expressions;

namespace shabs.DataQuery.Abstractions;

public interface IQueryInclude
{
    LambdaExpression NavigationLambdaExpression { get; }
    Type GetPropertyType();
    IQueryInclude? GetThenInclude(); 
}