using System.Linq.Expressions;

namespace QueryInfo.Models;

public abstract class QueryInfoFilterRule
{
    public abstract Expression<Func<T, bool>> ToQueryInfoFilter<T>();
}
