using LinqApi.Logging.Log;
using LinqApi.Epoch;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LinqApi.Logging
{
    public class LinqLoggingBehavior
    {
        private readonly IEpochProvider _epochProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LinqLoggingBehavior(IEpochProvider epochProvider, IHttpContextAccessor httpContextAccessor)
        {
            _epochProvider = epochProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public void ApplyLoggingRules(ChangeTracker tracker)
        {
            foreach (var entry in tracker.Entries<LinqLogEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.Epoch = _epochProvider.GetEpoch(entry.Entity.CreatedAt);
                }

                if (entry.State == EntityState.Modified)
                {
                    var user = _httpContextAccessor?.HttpContext?.User;
                    var userId = user?.FindFirst("userprofile.id")?.Value;
                    if (long.TryParse(userId, out var id))
                        entry.Entity.CreatedBy = id;
                }
            }
        }
    }
}
