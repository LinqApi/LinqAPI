using LinqApi.Localization;
using LinqApi.Localization.LinqApi.Localization;
using Microsoft.EntityFrameworkCore;

namespace LinqApi.Repository
{
    public class LocalizationRepository : ILocalizationRepository
    {
        private readonly LinqLocalizationDbContext _dbContext;

        public LocalizationRepository(LinqLocalizationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<LocalizationEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.LocalizationEntries.ToListAsync(cancellationToken);
        }

        public async Task<LocalizationEntity> UpsertAsync(LocalizationEntity entity, CancellationToken cancellationToken)
        {
            // Simple upsert logic. Extend with proper key checks as needed.
            var existing = await _dbContext.LocalizationEntries.FindAsync(entity.Id);
            if (existing == null)
            {
                _dbContext.LocalizationEntries.Add(entity);
            }
            else
            {
                _dbContext.Entry(existing).CurrentValues.SetValues(entity);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }
    }
}
