using System.Linq.Expressions;

namespace shlabs.DataQuery.Abstractions.Utils
{
    public static class Extensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var body = Expression.AndAlso(
                Expression.Invoke(left, parameter),
                Expression.Invoke(right, parameter)
            );
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var body = Expression.OrElse(
                Expression.Invoke(left, parameter),
                Expression.Invoke(right, parameter)
            );
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<TEntity, IEnumerable<TProperty>?>> ToEnumerableExpression<TEntity, TProperty>(
            this Expression<Func<TEntity, ICollection<TProperty>?>> source)
        {
            // We take the existing body and parameters and wrap them 
            // in the new Func signature.
            return Expression.Lambda<Func<TEntity, IEnumerable<TProperty>?>>(
                source.Body,
                source.Parameters
            );
        }
    }
}
