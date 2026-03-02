using shlabs.DataQuery.Abstractions.Utils;
using System.Collections;
using System.Linq.Expressions;
using static shlabs.DataQuery.Abstractions.Dynamic.QueryBuilderFilter;

namespace shlabs.DataQuery.Abstractions;

public class QueryBuilder<TEntity> : CoreQueryBuilder
    where TEntity : class
{
    private List<IQueryInclude<TEntity>>? _includes = new();

    public Expression<Func<TEntity, bool>>? Filter { get; set; }

    public IReadOnlyList<IQueryInclude<TEntity>>? Includes => _includes?.AsReadOnly();

    public IQueryOrder<TEntity>? Order { get; set; }

    public QueryBuilder()
    {
    }
    
    public QueryBuilder(Expression<Func<TEntity, bool>>? filter)
    {
        Filter = filter;
    }
        
    #region Include

    public void Include(IQueryInclude<TEntity> include)
    {
        _includes ??= [];
        _includes.Add(include);
    }
    
    public void Include(IEnumerable<IQueryInclude<TEntity>> include)
    {
        foreach (var incl in include)
        {
            Include(incl);
        }
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
        var includeInfo = new QueryIncludeList<TEntity, TProperty>(property, filter);
        _includes.Add(includeInfo);

        return includeInfo;
    }

    public QueryBuilder<TEntity> AddInclude(IQueryInclude<TEntity> include)
    {
        Include(include);
        return this;
    }
    
    public QueryBuilder<TEntity> AddInclude(IEnumerable<IQueryInclude<TEntity>> include)
    {
        Include(include);
        return this;
    }
    
    public QueryBuilder<TEntity> AddIncludeEntity<TProperty>(Expression<Func<TEntity, TProperty?>> property)
        where TProperty : class
    {
        IncludeEntity(property);
        return this;
    }

    public QueryBuilder<TEntity> AddIncludeList<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>?>> property, Expression<Func<TProperty, bool>>? filter = null)
        where TProperty : class
    {
        IncludeList(property, filter);
        return this;
    }
    
    public QueryBuilder<TEntity> AddIncludeList<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>?>> property, Func<QueryIncludeList<TEntity, TProperty>, QueryIncludeList<TEntity, TProperty>> config)
        where TProperty : class
    {
        _includes ??= new();
        var includeInfo = new QueryIncludeList<TEntity, TProperty>(property, null);
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

    public QueryBuilder<TEntity> AppendFilter(Expression<Func<TEntity, bool>>? filter, Conditions condition = Conditions.And)
    {
        if (filter is null)
        {
            return this;
        }
        
        if (Filter is null)
        {
            Filter = filter;
        }
        else
        {
            if (condition == Conditions.And)
            {
                Filter = Filter.And(filter);
            }
            else
            {
                Filter = Filter.Or(filter);
            }
        }
        return this;
    }

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

    public QueryBuilder<TEntity> Clone()
    {
        var cloned = new QueryBuilder<TEntity>
        {
            Filter = Filter,
            Order = Order,
            Take = Take,
            Skip = Skip,
            _includes = _includes != null ? new List<IQueryInclude<TEntity>>(_includes) : null
        };

        return cloned;
    }
}