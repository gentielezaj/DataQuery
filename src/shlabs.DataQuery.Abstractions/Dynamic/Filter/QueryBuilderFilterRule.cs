using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions.Dynamic;

public abstract class QueryBuilderFilterRule
{
    public abstract Expression<Func<T, bool>> ToQueryBuilderFilter<T>();
}