using LinqApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace LinqApi.Repository
{
    public class LinqRepository<TDbContext, TEntity, TId> : ILinqRepository<TEntity, TId>
      where TDbContext : DbContext
      where TEntity : class
    {
        protected readonly TDbContext DbContext;
        protected readonly DbSet<TEntity> DbSet;

        public LinqRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
            DbSet = dbContext.Set<TEntity>();
        }

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbContext.AddAsync(entity, cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
            await SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                DbContext.Entry(entity).State = EntityState.Deleted;
                await SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<TEntity> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await DbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await DbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<PaginationModel<dynamic>> GetFilterPagedAsync(
      LinqFilterModel filterModel,
      CancellationToken cancellationToken = default)
        {
            // 1️⃣ DbSet üzerinden IQueryable<TEntity> oluşturuyoruz.
            IQueryable<TEntity> query = DbSet.AsQueryable();

            // 2️⃣ Dinamik Where (Filter) uygulaması
            if (!string.IsNullOrWhiteSpace(filterModel.Filter))
            {
                var predicate = DynamicExpressionParser.ParseLambda<TEntity, bool>(null, false, filterModel.Filter);
                query = query.Where((Expression<Func<TEntity, bool>>)predicate);
            }

            // 3️⃣ Include ve ThenInclude işlemleri (navigation paging EF Core’da direkt desteklenmediğinden sadece include path ekliyoruz)
            if (filterModel.Includes != null && filterModel.Includes.Any())
            {
                foreach (var include in filterModel.Includes)
                {
                    string includePath = include.PropertyName;
                    if (include.ThenIncludes != null && include.ThenIncludes.Any())
                    {
                        foreach (var thenInclude in include.ThenIncludes)
                        {
                            foreach (var childInclude in thenInclude.ChildIncludes)
                            {
                                string fullInclude = $"{includePath}.{childInclude.PropertyName}";
                                query = query.Include(fullInclude);
                            }
                        }
                    }
                    else
                    {
                        query = query.Include(includePath);
                    }
                }
            }

            // 4️⃣ Sorguyu dinamik işlemleri destekleyecek non-generic IQueryable'a dönüştürüyoruz.
            IQueryable dynamicQuery = query;

            // 5️⃣ GroupBy uygulaması (varsa)
            if (!string.IsNullOrWhiteSpace(filterModel.GroupBy))
            {
                // GroupBy sonrası sunucu tarafı çeviri desteklenmediğinden,
                // grouping sonucu client side'a çekmek için AsEnumerable() kullanıyoruz.
                dynamicQuery = dynamicQuery.GroupBy(filterModel.GroupBy)
                                           .AsQueryable();
            }

            // 6️⃣ Select/projection uygulaması (varsa)
            if (!string.IsNullOrWhiteSpace(filterModel.Select))
            {
                dynamicQuery = dynamicQuery.Select(filterModel.Select);
            }

            // 7️⃣ OrderBy uygulaması (dinamik)
            if (!string.IsNullOrWhiteSpace(filterModel.OrderBy))
            {
                string orderExpr = filterModel.Desc ? $"{filterModel.OrderBy} descending" : filterModel.OrderBy;
                dynamicQuery = dynamicQuery.OrderBy(orderExpr);
            }

            // 8️⃣ IQueryable'ı generic hale getiriyoruz.
            var castQuery = dynamicQuery.Cast<dynamic>();

            // 9️⃣ Toplam kayıt sayısını alıyoruz (client-side count, grouping sonrası zaten in-memory)
            int totalCount = castQuery.Count();

            // 10️⃣ Paging uygulaması (ana sorgu için)
            int skip = (filterModel.Pager.PageNumber - 1) * filterModel.Pager.PageSize;
            var pagedQuery = castQuery.Skip(skip).Take(filterModel.Pager.PageSize);

            // 11️⃣ Sorguyu çalıştırıp sonuçları listeliyoruz
            // Not: Bu noktada sorgu in-memory olduğu için asenkron fayda sağlamayabilir.
            var items = pagedQuery.ToList();

            return new PaginationModel<dynamic>
            {
                Items = items,
                TotalRecords = totalCount
            };
        }



    }


}
