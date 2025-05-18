// Ignore Spelling: Queryable

using Microsoft.EntityFrameworkCore.Query;

namespace QueryInfo.Models.IncludeModels;

public interface IThenIncludeInfo<in TPreviousProperty>
{
    IQueryable<TSource> ToQueryableEntity<TSource>(IIncludableQueryable<TSource, TPreviousProperty> source)
        where TSource : class;

    IQueryable<TSource> ToQueryableList<TSource>(IIncludableQueryable<TSource, IEnumerable<TPreviousProperty>> source)
        where TSource : class;
}
