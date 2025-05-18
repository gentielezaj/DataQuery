using System.Linq.Expressions;

namespace QueryInfo.Models;


public class QueryInfoFilter : QueryInfoFilterRule
{
    public List<QueryInfoFilterRule>? Rules { get; set; } = new();
    public Conditions Condition { get; set; }

    public override Expression<Func<T, bool>> ToQueryInfoFilter<T>()
    {
        if (Rules == null || !Rules.Any())
        {
            return x => true; // No rules means no filtering
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        foreach (var rule in Rules)
        {
            var ruleExpression = rule.ToQueryInfoFilter<T>();
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

    public static QueryInfoFilter And<T>(params T[] rules)
        where T : QueryInfoFilterRule
    {
        return new QueryInfoFilter
        {
            Rules = rules.Cast<QueryInfoFilterRule>().ToList(),
            Condition = Conditions.And
        };
    }

    public static QueryInfoFilter Or<T>(params T[] rules)
        where T : QueryInfoFilterRule
    {
        return new QueryInfoFilter
        {
            Rules = rules.Cast<QueryInfoFilterRule>().ToList(),
            Condition = Conditions.Or
        };
    }

    public enum Conditions
    {
        And,
        Or
    }
}
