using LinqApi.Model;
using Microsoft.EntityFrameworkCore;

namespace LinqApi.Helpers
{
    public static class PaginationHelper
    {
        /// <summary>
        /// Generates a Paginated Data Async
        /// </summary>
        /// <param name="dbQuery">IQueryable data to be paginated</param>
        /// <param name="pager">Pagination Filter</param>
        /// <returns></returns>
        public static async Task<PaginationModel<T>> PaginateAsync<T>(this IQueryable<T> dbQuery, Pager pager = null)
        {
            pager = pager ?? new Pager();
            var data = await dbQuery
                .Skip((pager.PageNumber - 1) * pager.PageSize)
                .Take(pager.PageSize)
                .ToListAsync();

            return new PaginationModel<T>
            {
                Items = data,
                TotalRecords = await dbQuery.CountAsync()
            };
        }

        /// <summary>
        /// Generates a Paginated Data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbQuery"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        public static PaginationModel<T> Paginate<T>(this IQueryable<T> dbQuery, Pager pager = null)
        {
            pager = pager ?? new Pager();
            var data = dbQuery
                .Skip((pager.PageNumber - 1) * pager.PageSize)
                .Take(pager.PageSize)
                .ToList();

            return new PaginationModel<T>
            {
                Items = data,
                TotalRecords = dbQuery.Count()
            };
        }
    }
}
