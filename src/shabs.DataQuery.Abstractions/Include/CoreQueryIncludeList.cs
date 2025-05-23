using System.Linq.Expressions;

namespace shabs.DataQuery.Abstractions;

public abstract class CoreQueryIncludeList<TEntity, TProperty>(
    Expression<Func<TEntity, IEnumerable<TProperty>?>> navigation,
    Expression<Func<TProperty, bool>>? filter,
    IThenQueryInclude<TProperty>? thenIncludes = null)
    : CoreQueryInclude<TEntity, IEnumerable<TProperty>, TProperty>(navigation, thenIncludes)
    where TEntity : class
    where TProperty : class
{
    public Expression<Func<TProperty, bool>>? Filter { get; set; } = filter;
    public int? Take { get; set; }
    public int? Skip { get; set; }
    public IQueryOrder<TProperty>? Order { get; set; }
}