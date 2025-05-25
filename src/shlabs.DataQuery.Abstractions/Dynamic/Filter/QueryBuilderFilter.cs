using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions.Dynamic;

public class QueryBuilderFilter : QueryBuilderFilterRule
{
    public List<QueryBuilderFilterRule>? Rules { get; set; } = new();
    public Conditions Condition { get; set; }

    public override Expression<Func<T, bool>> ToQueryBuilderFilter<T>()
    {
        if (Rules == null || !Rules.Any())
        {
            return x => true; // No rules means no filtering
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        foreach (var rule in Rules)
        {
            var ruleExpression = rule.ToQueryBuilderFilter<T>();
            var invokedExpression = Expression.Invoke(ruleExpression, parameter);

            if (combined == null)
            {
                combined = invokedExpression;
            }
            else
            {
                combined = Condition switch
                {
                    Conditions.And => Expression.AndAlso(combined, invokedExpression),
                    Conditions.Or => Expression.OrElse(combined, invokedExpression),
                    _ => throw new Exception($"Condition '{Condition}' is not supproted.")
                };
            }
        }

        return Expression.Lambda<Func<T, bool>>(combined!, parameter);
    }

    public static QueryBuilderFilter And<T>(params T[] rules)
        where T : QueryBuilderFilterRule
    {
        return new QueryBuilderFilter
        {
            Rules = rules.Cast<QueryBuilderFilterRule>().ToList(),
            Condition = Conditions.And
        };
    }

    public static QueryBuilderFilter Or<T>(params T[] rules)
        where T : QueryBuilderFilterRule
    {
        return new QueryBuilderFilter
        {
            Rules = rules.Cast<QueryBuilderFilterRule>().ToList(),
            Condition = Conditions.Or
        };
    }

    public enum Conditions
    {
        And,
        Or
    }
}