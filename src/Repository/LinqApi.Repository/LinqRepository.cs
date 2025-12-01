using LinqApi.Core;
using LinqApi.Core;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace LinqApi.Repository
{
    /// <summary>
    /// Represents a generic repository that provides LINQ-based data access for entities.
    /// This repository is solely responsible for CRUD and query operations on TEntity.
    /// Any logging, caching, or additional behavior should be implemented by subscribing
    /// to its events (e.g. via decorators or interceptors).
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    public class LinqRepository<TDbContext, TEntity, TId> : ILinqRepository<TEntity, TId>
    where TDbContext : DbContext
    where TEntity : BaseEntity<TId>
    {
        /// <summary>
        /// Gets the database context instance.
        /// </summary>
        protected readonly TDbContext DbContext;

        /// <summary>
        /// Gets the <see cref="DbSet{TEntity}"/> instance.
        /// </summary>
        protected readonly DbSet<TEntity> DbSet;


        /// <summary>
        /// Initializes a new instance of the <see cref="LinqRepository{TDbContext, TEntity, TId}"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public LinqRepository(TDbContext dbContext)
        {
            DbContext = dbContext;
            DbSet = dbContext.Set<TEntity>();
            
        }

        /// <inheritdoc/>
        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbContext.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            // Raise event for external observers (e.g. logging or caching interceptors)
            return entity;
        }

        /// <inheritdoc/>
        public virtual IQueryable<TEntity> Query()
        {
            return DbSet.AsQueryable();
        }

        /// <inheritdoc/>
        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            // Raise event for external observers
            return entity;
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, null,cancellationToken).ConfigureAwait(false);
            if (entity != null)
            {
                DbContext.Entry(entity).State = EntityState.Deleted;
                await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                // Raise event for external observers
            }
        }

        public async Task<int> DeleteWhereAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default)
        {
            // Predicate'e uyan tüm kayıtları çek
            var entities = await DbSet.Where(predicate).ToListAsync(cancellationToken);

            if (!entities.Any())
                return 0;

            // Toplu silme işlemi
            DbSet.RemoveRange(entities);

            // SaveChanges varsa burada ya da dışarıdan çağrılır
            return await SaveChangesAsync(cancellationToken);
        }
        /// <inheritdoc/>
        public async Task<TEntity?> GetByIdAsync(
     TId id,
     IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>>? includeFunctions = null,
     CancellationToken cancellationToken = default)
        {
	        IQueryable<TEntity> query = Query();

            if (includeFunctions != null)
            {
                foreach (var include in includeFunctions)
                {
                    query = include(query);
                }
            }

            // FindAsync yerine filtreli FirstOrDefault
            return await query.FirstOrDefaultAsync(x => x.Id!.Equals(id), cancellationToken)
                              .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await DbSet.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<TEntity>> FindManyWithFilterAsync(
    Expression<Func<TEntity, bool>> predicate,
    IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>> includeFunctions,
    CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = DbSet.AsNoTracking().AsNoTracking();

            if (includeFunctions != null)
            {
                foreach (var include in includeFunctions)
                {
                    query = include(query);
                }
            }

            return await query
                .Where(predicate)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }


        /// <inheritdoc/>
        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
     await DbSet.AsNoTracking().Where(predicate).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
           await DbSet.AsNoTracking().AnyAsync(predicate, cancellationToken).ConfigureAwait(false);

        public async Task<(bool found, TEntity? entity)> TryFindFastAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default)
        {
            // Bu metot sadece Id'yi çekiyor
            var idOnly = await DbSet.AsNoTracking()
                .Where(predicate)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (EqualityComparer<TId>.Default.Equals(idOnly, default))
                return (false, null);

            var entity = await DbSet.AsNoTracking().FirstAsync(x => x.Id!.Equals(idOnly), cancellationToken);
            return (true, entity);
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        /// <remarks>
        /// This method applies dynamic filtering, includes, grouping, projection, ordering, and paging.
        /// It uses the <see cref="System.Linq.Dynamic.Core"/> library to parse dynamic expressions.
        /// For better adherence to the Single Responsibility Principle, consider extracting the filtering logic
        /// into a separate service.
        /// </remarks>
        public async Task<PaginationModel<dynamic>> GetFilterPagedAsync(
            LinqFilterModel filterModel,
            CancellationToken cancellationToken = default)
        {
            // Ensure a valid Pager instance is provided.
            if (filterModel.Pager == null)
            {
                filterModel.Pager = new Pager { PageNumber = 1, PageSize = 1 };
            }

            // 1️⃣ Create an IQueryable<TEntity> from the DbSet.AsNoTracking().
            IQueryable<TEntity> query = Query();

            // 2️⃣ Parameterize the filter string.
            (string paramFilter, List<object> parameters) = ParameterizeFilterString(filterModel.Filter);

            // 3️⃣ Apply dynamic filtering if a filter is provided.
            if (!string.IsNullOrWhiteSpace(paramFilter))
            {
                var predicate = DynamicExpressionParser.ParseLambda<TEntity, bool>(
                    ParsingConfig.DefaultEFCore21,
                    false,
                    paramFilter,
                    parameters.ToArray()
                );
                query = query.Where(predicate);
            }

            // 4️⃣ Apply dynamic include paths for navigation properties.
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

            // 5️⃣ Convert to non-generic IQueryable for dynamic operations.
            IQueryable dynamicQuery = query;

            // 6️⃣ Apply grouping if specified.
            if (!string.IsNullOrWhiteSpace(filterModel.GroupBy))
            {
                // Use AsEnumerable() to perform grouping on the client side.
                dynamicQuery = dynamicQuery.GroupBy(filterModel.GroupBy).AsQueryable();
            }

            // 7️⃣ Apply dynamic projection (select).
            if (!string.IsNullOrWhiteSpace(filterModel.Select))
            {
                dynamicQuery = dynamicQuery.Select(filterModel.Select);
            }

            // 8️⃣ Apply dynamic ordering.
            if (!string.IsNullOrWhiteSpace(filterModel.OrderBy))
            {
                string orderExpr = filterModel.Desc ? $"{filterModel.OrderBy} descending" : filterModel.OrderBy;
                dynamicQuery = dynamicQuery.OrderBy(orderExpr);
            }

            // 9️⃣ Cast dynamicQuery to IQueryable<dynamic>.
            var castQuery = dynamicQuery.Cast<dynamic>();


            // 🔟 Get the total record count.
            int totalCount = await castQuery.CountAsync(cancellationToken).ConfigureAwait(false);

            // ----------------------------------------------------
            // ✅ Development: count 0 ise erken dönüş yapıyoruz
            // ----------------------------------------------------
            if (totalCount == 0)
            {
                return new PaginationModel<dynamic>
                {
                    Items = new List<dynamic>(),
                    TotalRecords = 0
                };
            }

            // 1️⃣1️⃣ Apply paging.
            int skip = (filterModel.Pager.PageNumber - 1) * filterModel.Pager.PageSize;
            var pagedQuery = castQuery.Skip(skip).Take(filterModel.Pager.PageSize);

            // 1️⃣2️⃣ Execute the query and get the results.
            var items = await pagedQuery.ToListAsync(cancellationToken).ConfigureAwait(false);

            return new PaginationModel<dynamic>
            {
                Items = items,
                TotalRecords = totalCount
            };
        }

        public async Task<TEntity?> FindWithFilterAsync(
      Expression<Func<TEntity, bool>> predicate,
      IEnumerable<Func<IQueryable<TEntity>, IQueryable<TEntity>>> includeFunctions,
      CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = DbSet.AsNoTracking();

            if (includeFunctions != null)
            {
                foreach (var include in includeFunctions)
                {
                    query = include(query);
                }
            }

            return await query
                .FirstOrDefaultAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Parameterizes a dynamic filter string by replacing literal values with parameter placeholders.
        /// </summary>
        /// <param name="filter">The filter string containing literal values.</param>
        /// <returns>
        /// A tuple containing the parameterized filter string and a list of corresponding parameter values.
        /// </returns>
        /// <remarks>
        /// This method uses regular expressions to replace literal values in the filter string with placeholders.
        /// Numeric and date literal values are detected and converted to their appropriate types.
        /// </remarks>
        private static (string, List<object>) ParameterizeFilterString(string filter)
        {
            var parameters = new List<object>();
            if (string.IsNullOrWhiteSpace(filter))
            {
                return (filter, parameters);
            }

            // 1️⃣ Replace literal values enclosed in double quotes with parameter placeholders.
            string newFilter = Regex.Replace(filter, "\"([^\"]+)\"", match =>
            {
                string literal = match.Groups[1].Value;
                // If the literal matches a date format (e.g., "yyyy-MM-dd"), parse it as DateTime.
                if (Regex.IsMatch(literal, @"^\d{4}-\d{2}-\d{2}$"))
                {
                    parameters.Add(DateTime.ParseExact(literal, "yyyy-MM-dd", CultureInfo.InvariantCulture));
                }
                else
                {
                    parameters.Add(literal);
                }
                return "@" + (parameters.Count - 1);
            });

            // 2️⃣ Replace numeric literal values with parameter placeholders.
            // The negative lookbehind (?<!@) ensures that already replaced placeholders are not matched.
            newFilter = Regex.Replace(newFilter, @"(?<!@)\b\d+(\.\d+)?\b", match =>
            {
                if (int.TryParse(match.Value, out int intValue))
                {
                    parameters.Add(intValue);
                }
                else if (double.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
                {
                    parameters.Add(doubleValue);
                }
                else
                {
                    parameters.Add(match.Value);
                }
                return "@" + (parameters.Count - 1);
            });

            return (newFilter, parameters);
        }
    }
}
