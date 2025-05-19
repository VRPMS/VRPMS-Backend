using System.Linq.Expressions;

namespace VRPMS.Common.Extensions;

public static class LinqExtensions
{
    public static IQueryable<TSource> WhereIfHasValue<TSource, TValue>(
        this IQueryable<TSource> source,
        TValue? value,
        Expression<Func<TSource, bool>> predicate)
        where TValue : struct
    {
        if (value.HasValue)
        {
            return source.Where(predicate);
        }

        return source;
    }
}
