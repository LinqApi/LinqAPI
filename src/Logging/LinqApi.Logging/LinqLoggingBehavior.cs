using LinqApi.Logging.Log;
using LinqApi.Epoch;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LinqApi.Logging
{

    public interface ILogRule
    {
        void Apply(EntityEntry<LinqLogEntity> entry);
    }

    public class TimestampLogRule : ILogRule
    {
        private readonly IEpochProvider _epochProvider;

        public TimestampLogRule(IEpochProvider epochProvider)
        {
            _epochProvider = epochProvider;
        }

        public void Apply(EntityEntry<LinqLogEntity> entry)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.Epoch = _epochProvider.GetEpoch(entry.Entity.CreatedAt);
            }
        }
    }

    public class CreatedByLogRule : ILogRule
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreatedByLogRule(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Apply(EntityEntry<LinqLogEntity> entry)
        {
            if (entry.State == EntityState.Modified)
            {
                var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst("userprofile.id")?.Value;
                if (long.TryParse(userId, out var id))
                    entry.Entity.CreatedBy = id;
            }
        }
    }

    public class LinqLoggingBehavior
    {
        private readonly IEnumerable<ILogRule> _rules;

        public LinqLoggingBehavior(IEnumerable<ILogRule> rules)
        {
            _rules = rules;
        }

        public void ApplyLoggingRules(ChangeTracker tracker)
        {
            foreach (var entry in tracker.Entries<LinqLogEntity>())
            {
                foreach (var rule in _rules)
                    rule.Apply(entry);
            }
        }
    }
}
