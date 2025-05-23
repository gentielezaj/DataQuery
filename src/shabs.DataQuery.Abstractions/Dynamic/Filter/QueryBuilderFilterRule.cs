using System.Linq.Expressions;

namespace shabs.DataQuery.Abstractions.Dynamic;

public abstract class QueryBuilderFilterRule
{
    public abstract Expression<Func<T, bool>> ToQueryBuilderFilter<T>();
}