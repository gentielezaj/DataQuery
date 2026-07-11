using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions.Dynamic;

public abstract class QueryBuilderFilterRule
{
    public abstract Expression<Func<T, bool>> ToQueryBuilderFilter<T>(QueryBuilderFilterRuleConvertorOptions? options = null);
}

public record QueryBuilderFilterRuleConvertorOptions(
    DateTimeKind? DateTimeKind = null);