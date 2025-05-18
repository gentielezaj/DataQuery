// Ignore Spelling: Queryable

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace QueryInfo.Models.IncludeModels;

public class IncludeInfoEntity<TEntity, TProperty>(
    Expression<Func<TEntity, TProperty?>> navigation,
    IThenIncludeInfo<TProperty>? thenIncludes = null)
    : CoreIncludeInfo<TEntity, TProperty, TProperty>(navigation, thenIncludes), IIncludeInfo<TEntity>
    where TEntity : class
    where TProperty : class
{
    public IQueryable<TEntity> ToQueryable(IQueryable<TEntity> source)
    {
        var include = source.Include(Navigation);

        return ThenIncludes is null
            ? include
            : ThenIncludes.ToQueryableEntity(include!);
    }
}
