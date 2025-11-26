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
    }
}
