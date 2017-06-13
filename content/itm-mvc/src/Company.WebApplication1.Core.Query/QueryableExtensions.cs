using System;
using System.Linq;
namespace Company.WebApplication1.Core.Query
{
    /// <summary>
    /// This class is resposible for defining general extensions for the <see cref="IQueryable"/> interface.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Paginates a query.
        /// </summary>
        /// <remarks>
        /// Remember to always <see cref="OrderBy()"/> your query when paging.
        /// Else the order isn't guaranteed.
        /// </remarks>
        /// <param name="query">The query you wish to paginate.</param>
        /// <param name="page">The page number (starting from 1) you wish to retrieve.</param>
        /// <param name="pageSize">The number of items on a page.</param>
        /// <returns>Returns a paginated query.</returns>
        public static IQueryable<T> Paged<T>(this IQueryable<T> query, int page, int pageSize)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), page, "Value must be greater than 0.");

            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Value must be greater than 0.");

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
    }
}
