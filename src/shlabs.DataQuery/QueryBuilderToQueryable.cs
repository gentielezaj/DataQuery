using Microsoft.EntityFrameworkCore;
using shlabs.DataQuery.Abstractions;

namespace System.Linq;

public static class QueryBuilderToQueryable
{
    public static IQueryable<T> ToQueryable<T>(this QueryBuilder<T> builder, DbContext dbContext)
        where T : class
    {
        if (dbContext is null)
        {
            throw new ArgumentNullException(nameof(dbContext));
        }

        var dbSet = dbContext.Set<T>();
        return ToQueryable(builder, dbSet);
    }
    
    public static IQueryable<T> ToQueryable<T>(this QueryBuilder<T> builder, DbSet<T> dbSet)
        where T : class
    {
        var query = dbSet.AsQueryable();
        return ToQueryable(builder, query);
    }
    
    public static IQueryable<T> ToQueryable<T>(this QueryBuilder<T> builder, IQueryable<T> query) 
        where T : class
    {
        if (builder.Filter is not null)
        {
            query = query.Where(builder.Filter);
        }

        if (builder.Order is not null)
        {
            //query = OrderBy(query, Order.ToArray());
            query = QueryOrderToQueryable.ToQueryable(builder.Order, query);
        }

        if (builder.Skip.HasValue)
        {
            query = query.Skip(builder.Skip.Value);
        }

        if (builder.Take.HasValue)
        {
            query = query.Take(builder.Take.Value);
        }

        if (builder.Includes?.Any() == true)
        {
            foreach (var includeProperties in builder.Includes)
            {
                query = QueryIncludeToQueryable.ToQueryable(includeProperties, query);
            }
        }

        return query;
    }
}