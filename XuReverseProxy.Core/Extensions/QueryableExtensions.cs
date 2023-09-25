using System.Linq.Expressions;

namespace XuReverseProxy.Core.Extensions;

public static class QueryableExtensions
{
    public static IOrderedQueryable<TSource> SortByWithToggledDirection<TSource, TKey>(this IQueryable<TSource> query, Expression<Func<TSource, TKey>> keySelector, bool descending)
        => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
}
