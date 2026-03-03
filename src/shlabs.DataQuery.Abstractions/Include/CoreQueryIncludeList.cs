using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions;

public abstract class CoreQueryIncludeList<TEntity, TProperty>(
    Expression<Func<TEntity, IEnumerable<TProperty>?>> navigation,
    Expression<Func<TProperty, bool>>? filter,
    IThenQueryInclude<TProperty>? thenIncludes = null)
    : QueryInclude<TEntity, IEnumerable<TProperty>, TProperty>(navigation, thenIncludes)
    where TEntity : class
    where TProperty : class
{
    public Expression<Func<TProperty, bool>>? Filter { get; set; } = filter;
    public int? Take { get; set; }
    public int? Skip { get; set; }
    public IQueryOrder<TProperty>? Order { get; set; }

    public void SetOrder(params IQueryOrder<TProperty>[] orders)
    {
        if (orders.Length == 0)
        {
            return;
        }

        var quqer = new Queue<IQueryOrder<TProperty>>(orders);
        Order = quqer.Dequeue();
        Order = Order.AddOrderBy(quqer);

        return;
    }
}