using System.Linq.Expressions;

namespace QueryInfo.Models;

public class QueryInfoFilterCriteria(
    string field,
    QueryInfoFilterConditionType @operator,
    string value)
    : QueryInfoFilterRule
{
    public string Field { get; set; } = field;
    public string Value { get; set; } = value;
    public QueryInfoFilterConditionType Operator { get; set; } = @operator;

    [Obsolete("This  constructior is used only by controller property resolver. Do not use it in your code.")]
    public QueryInfoFilterCriteria() : this(string.Empty, QueryInfoFilterConditionType.Equal, null)
    {
    }

    public override Expression<Func<T, bool>> ToQueryInfoFilter<T>()
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var member = Expression.Property(parameter, Field);
        var constant = Expression.Constant(Convert.ChangeType(Value, member.Type));

        Expression body = Operator switch
        {
            QueryInfoFilterConditionType.Equal => Expression.Equal(member, constant),
            QueryInfoFilterConditionType.NotEqual => Expression.NotEqual(member, constant),
            QueryInfoFilterConditionType.Greater => Expression.GreaterThan(member, constant),
            QueryInfoFilterConditionType.Less => Expression.LessThan(member, constant),
            QueryInfoFilterConditionType.GreaterOrEqual => Expression.GreaterThanOrEqual(member, constant),
            QueryInfoFilterConditionType.LessOrEqual => Expression.LessThanOrEqual(member, constant),
            QueryInfoFilterConditionType.Contains => Expression.Call(member,
                typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constant),
            QueryInfoFilterConditionType.NotContains => Expression.Not(Expression.Call(member,
                typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constant)),
            QueryInfoFilterConditionType.Null => Expression.Equal(member, Expression.Constant(null)),
            QueryInfoFilterConditionType.NotNull => Expression.NotEqual(member, Expression.Constant(null)),
            _ => throw new Exception($"Condition type '{Operator}' is not Supported.")
        };

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
