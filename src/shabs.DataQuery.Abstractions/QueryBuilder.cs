using System.Collections;
using System.Linq.Expressions;

namespace shabs.DataQuery.Abstractions;

public class QueryBuilder<TEntity> : CoreQueryBuilder
    where TEntity : class
{
    private List<IQueryInclude<TEntity>>? _includes = new();

    public Expression<Func<TEntity, bool>>? Filter { get; set; }

    public IReadOnlyList<IQueryInclude<TEntity>>? Includes => _includes?.AsReadOnly();

    public IQueryOrder<TEntity>? Order { get; set; }

    #region Include

    public void Include(IQueryInclude<TEntity> include)
    {
        _includes ??= [];
        _includes.Add(include);
    }

    public void Include<TProperty>(QueryIncludeEntity<TEntity, TProperty> include) where TProperty : class
    {
        _includes ??= [];
        _includes.Add(include);
    }

    public void Include<TProperty>(QueryIncludeList<TEntity, IEnumerable<TProperty>> include)
    {
        _includes ??= [];
        _includes.Add(include);
    }

    public QueryIncludeEntity<TEntity, TProperty> IncludeEntity<TProperty>(Expression<Func<TEntity, TProperty?>> property)
        where TProperty : class
    {
        var propertyType = typeof(TProperty);
        if (typeof(IEnumerable).IsAssignableFrom(propertyType))
        {
            throw new ArgumentException("Use IncludeList for collections");
        }

        _includes ??= new();
        var includeInfo = new QueryIncludeEntity<TEntity, TProperty>(property);
        _includes.Add(includeInfo);

        return includeInfo;
    }

    public QueryIncludeList<TEntity, TProperty> IncludeList<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>?>> property,
        Expression<Func<TProperty, bool>>? filter = null)
        where TProperty : class
    {
        _includes ??= new();
        var includeInfo = new QueryIncludeList<TEntity, TProperty>(property, filter, null);
        _includes.Add(includeInfo);

        return includeInfo;
    }

    public QueryBuilder<TEntity> AddIncludeEntity<TProperty>(Expression<Func<TEntity, TProperty?>> property)
        where TProperty : class
    {
        IncludeEntity<TProperty>(property);
        return this;
    }

    public QueryBuilder<TEntity> AddIncludeList<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>?>> property, Expression<Func<TProperty, bool>>? filter = null)
        where TProperty : class
    {
        IncludeList<TProperty>(property, filter);
        return this;
    }
    
    public QueryBuilder<TEntity> AddIncludeList<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>?>> property, Func<QueryIncludeList<TEntity, TProperty>, QueryIncludeList<TEntity, TProperty>> config)
        where TProperty : class
    {
        _includes ??= new();
        var includeInfo = new QueryIncludeList<TEntity, TProperty>(property, null, null);
        includeInfo = config(includeInfo);
        _includes.Add(includeInfo);
        
        return this;
    }

    #endregion include

    #region order

    public IQueryOrder<TEntity> SetOrderBy<TProperty>(Expression<Func<TEntity, TProperty>> selector, QueryOrderDirections direction)
    {
        Order = new QueryOrder<TEntity, TProperty>(selector, direction);
        return Order;
    }

    public QueryBuilder<TEntity> AddOrderBy<TProperty>(Expression<Func<TEntity, TProperty>> selector,
        QueryOrderDirections direction)
    {
        var order = new QueryOrder<TEntity, TProperty>(selector, direction);
        if (Order is null)
        {
            Order = order;
        }
        else
        {
             Order.AddOrderBy(order);
        }
        
        return this;
    }

    public QueryBuilder<TEntity> SetOrder(params IQueryOrder<TEntity>[] orders)
    {
        if (orders.Length == 0)
        {
            return this;
        }

        var quqer = new Queue<IQueryOrder<TEntity>>(orders);
        Order = quqer.Dequeue();
        Order = Order.AddOrderBy(quqer);

        return this;
    }

    #endregion

    public QueryBuilder<TEntity> SetTake(int? take)
    {
        Take = take;
        return this;
    }

    public QueryBuilder<TEntity> SetSkip(int? skip)
    {
        Skip = skip;
        return this;
    }

    public QueryBuilder<TEntity> SetFilter(Expression<Func<TEntity, bool>> filter)
    {
        Filter = filter;
        return this;
    }
}