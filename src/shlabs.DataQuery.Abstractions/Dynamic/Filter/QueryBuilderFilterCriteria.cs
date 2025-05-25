using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions.Dynamic;

public class QueryBuilderFilterCriteria(
    string field,
    QueryBuilderCriteriaConditions @operator,
    string value)
    : QueryBuilderFilterRule
{
    public string Field { get; set; } = field;
    public string Value { get; set; } = value;
    public QueryBuilderCriteriaConditions Operator { get; set; } = @operator;

    [Obsolete("This  constructior is used only by controller property resolver. Do not use it in your code.")]
    public QueryBuilderFilterCriteria() : this(string.Empty, QueryBuilderCriteriaConditions.Equal, null)
    {
    }

    public override Expression<Func<T, bool>> ToQueryBuilderFilter<T>()
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var member = Expression.Property(parameter, Field);
        var constant = Expression.Constant(Convert.ChangeType(Value, member.Type));

        Expression body = Operator switch
        {
            QueryBuilderCriteriaConditions.Equal => Expression.Equal(member, constant),
            QueryBuilderCriteriaConditions.NotEqual => Expression.NotEqual(member, constant),
            QueryBuilderCriteriaConditions.Greater => Expression.GreaterThan(member, constant),
            QueryBuilderCriteriaConditions.Less => Expression.LessThan(member, constant),
            QueryBuilderCriteriaConditions.GreaterOrEqual => Expression.GreaterThanOrEqual(member, constant),
            QueryBuilderCriteriaConditions.LessOrEqual => Expression.LessThanOrEqual(member, constant),
            QueryBuilderCriteriaConditions.Contains => Expression.Call(member,
                typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constant),
            QueryBuilderCriteriaConditions.NotContains => Expression.Not(Expression.Call(member,
                typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constant)),
            QueryBuilderCriteriaConditions.Null => Expression.Equal(member, Expression.Constant(null)),
            QueryBuilderCriteriaConditions.NotNull => Expression.NotEqual(member, Expression.Constant(null)),
            _ => throw new Exception($"Condition type '{Operator}' is not Supported.")
        };

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}