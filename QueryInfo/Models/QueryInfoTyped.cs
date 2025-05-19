using QueryInfo.Models.IncludeModels;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace QueryInfo.Models;

public class QueryInfo<TEntity> : CoreQueryInfo
    where TEntity : class
{
    private List<IIncludeInfo<TEntity>>? _includes = new();

    public Expression<Func<TEntity, bool>>? Where { get; set; }

    public IReadOnlyList<IIncludeInfo<TEntity>>? Includes => _includes?.AsReadOnly();

    public IOrderInfo<TEntity>? OrderInfo { get; set; }

    #region Include

    public void Include(IIncludeInfo<TEntity> include)
    {
        _includes ??= [];
        _includes.Add(include);
    }

    public void Include<TProperty>(IncludeInfoEntity<TEntity, TProperty> include) where TProperty : class
    {
        _includes ??= [];
        _includes.Add(include);
    }

    public void Include<TProperty>(IncludeInfoList<TEntity, IEnumerable<TProperty>> include)
    {
        _includes ??= [];
        _includes.Add(include);
    }

    public IncludeInfoEntity<TEntity, TProperty> IncludeEntity<TProperty>(Expression<Func<TEntity, TProperty?>> property)
        where TProperty : class
    {
        var propertyType = typeof(TProperty);
        if (typeof(IEnumerable).IsAssignableFrom(propertyType))
        {
            throw new ArgumentException("Use IncludeList for collections");
        }

        _includes ??= new();
        var includeInfo = new IncludeInfoEntity<TEntity, TProperty>(property);
        _includes.Add(includeInfo);

        return includeInfo;
    }

    public IncludeInfoList<TEntity, TProperty> IncludeList<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>?>> property,
        Expression<Func<TProperty, bool>>? filter = null)
        where TProperty : class
    {
        _includes ??= new();
        var includeInfo = new IncludeInfoList<TEntity, TProperty>(property, filter, null);
        _includes.Add(includeInfo);

        return includeInfo;
    }

    public QueryInfo<TEntity> SetIncludeEntity<TProperty>(Expression<Func<TEntity, TProperty?>> property)
        where TProperty : class
    {
        IncludeEntity<TProperty>(property);
        return this;
    }

    public QueryInfo<TEntity> SetIncludeList<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>?>> property, Expression<Func<TProperty, bool>>? filter = null)
        where TProperty : class
    {
        IncludeList<TProperty>(property, filter);
        return this;
    }

    #endregion include

    #region order

    public IOrderInfo<TEntity> Order<TProperty>(Expression<Func<TEntity, TProperty>> selector, OrderInfoDirections direction)
    {
        OrderInfo = new OrderInfo<TEntity, TProperty>(selector, direction);
        return OrderInfo;
    }

    public QueryInfo<TEntity> AddOrder<TProperty>(Expression<Func<TEntity, TProperty>> selector,
        OrderInfoDirections direction)
    {
        var order = new OrderInfo<TEntity, TProperty>(selector, direction);
        if (OrderInfo is null)
        {
            OrderInfo = order;
        }
        else
        {
             OrderInfo.AddOrderBy(order);
        }
        
        return this;
    }

    public QueryInfo<TEntity> SetOrder<TProperty>(Expression<Func<TEntity, TProperty>> selector, OrderInfoDirections direction)
        => SetOrder(new OrderInfo<TEntity, TProperty>(selector, direction));

    public QueryInfo<TEntity> SetOrder(params IOrderInfo<TEntity>[] orders)
    {
        if (orders.Length == 0)
        {
            return this;
        }

        var quqer = new Queue<IOrderInfo<TEntity>>(orders);
        OrderInfo = quqer.Dequeue();
        OrderInfo = OrderInfo.AddOrderBy(quqer);

        return this;
    }

    #endregion

    public QueryInfo<TEntity> SetTake(int? take)
    {
        Take = take;
        return this;
    }

    public QueryInfo<TEntity> SetSkip(int? skip)
    {
        Skip = skip;
        return this;
    }

    public QueryInfo<TEntity> SetWhere(Expression<Func<TEntity, bool>> where)
    {
        Where = where;
        return this;
    }

    public IQueryable<TEntity> ToQueryable(IQueryable<TEntity> query)
    {
        if (Where is not null)
        {
            query = query.Where(Where);
        }

        if (OrderInfo is not null)
        {
            //query = OrderBy(query, Order.ToArray());
            query = OrderInfo.ToQueryable(query);
        }

        if (Skip.HasValue)
        {
            query = query.Skip(Skip.Value);
        }

        if (Take.HasValue)
        {
            query = query.Take(Take.Value);
        }

        if (Includes is not null)
        {
            foreach (var includeProperties in Includes)
            {
                query = Include(query, includeProperties);
            }
        }

        return query;
    }

    #region include
    private IQueryable<TEntity> Include(IQueryable<TEntity> source, IIncludeInfo<TEntity>? includePropertiesData)
    {
        if (includePropertiesData is null)
        {
            return source;
        }

        return includePropertiesData.ToQueryable(source);
    }
    #endregion include
}
