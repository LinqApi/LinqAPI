using LinqApi.Epoch;
using LinqApi.Logging.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LinqApi.Logging
{
    public class LinqLoggingDbContext : DbContext, ILinqLoggingDbContextAdapter
    {
        private readonly string _schema;
        private readonly IEpochProvider _epochProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LinqLoggingDbContext (
     DbContextOptions<LinqLoggingDbContext> options,
     IEpochProvider epochProvider,
     IHttpContextAccessor? httpContextAccessor = null
 ) : base(options)
        {
            _epochProvider = epochProvider ?? new DefaultLinqEpochProvider();
            _httpContextAccessor = httpContextAccessor;
            _schema = "log";
        }

        public DbSet<LinqEventLog> EventLogs { get; set; }
        public DbSet<LinqHttpCallLog> HttpCallLogs { get; set; }
        public DbSet<LinqConsumeErrorLog> LinqConsumeErrorLogs { get; set; }
        public DbSet<LinqPublishErrorLog> LinqPublishErrorLogs { get; set; }
        public DbSet<LinqSqlLog> SqlLogs { get; set; }
        public DbSet<LinqSqlErrorLog> LinqDatabaseErrorLogs { get; set; }

        private long? GetUserProfileId ()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            var claim = user?.FindFirst("userprofile.id")?.Value;
            return long.TryParse(claim, out var id) ? id : null;
        }

        public override int SaveChanges ()
        {
            foreach (var entry in ChangeTracker.Entries<LinqLogEntity>())
            {

                UpdateLogTimestampsAndEpoch(entry);
                ModifiedDoes(entry);
            }
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync (CancellationToken cancellationToken = default)
        {

            foreach (var entry in ChangeTracker.Entries<LinqLogEntity>())
            {

                UpdateLogTimestampsAndEpoch(entry);
                ModifiedDoes(entry);
            }


            return await base.SaveChangesAsync(cancellationToken); // eski hali yanlış
        }

        private void ModifiedDoes (EntityEntry<LinqLogEntity> entry)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Epoch = _epochProvider.GetEpoch(entry.Entity.CreatedAt);

                if (entry.Entity.CreatedBy == 0 || entry.Entity.CreatedBy == null)
                {
                    var userId = GetUserProfileId();


                    if (userId.HasValue)
                        entry.Entity.CreatedBy = userId.Value;
                }
            }
        }

        private void UpdateLogTimestampsAndEpoch (EntityEntry<LinqLogEntity> entry)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.Now;
                entry.Entity.Epoch = _epochProvider.GetEpoch(entry.Entity.CreatedAt);
            }
        }

        protected override void OnModelCreating (ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Her alt sınıfı ayrı tabloya ve schema'ya yönlendir
            modelBuilder.Entity<LinqEventLog>().ToTable("EventLogs", "log");
            modelBuilder.Entity<InboundHttpCallLog>().ToTable("HttpCallInbound", "log");
            modelBuilder.Entity<OutboundHttpCallLog>().ToTable("HttpCallOutbound", "log");
            modelBuilder.Entity<OutboundHttpCallError>().ToTable("HttpCallOutboundError", "log");
            modelBuilder.Entity<HttpCallInboundError>().ToTable("HttpCallInboundError", "log");
            modelBuilder.Entity<LinqSqlLog>().ToTable("SqlLogs", "log");
            modelBuilder.Entity<LinqSqlErrorLog>().ToTable("SqlErrorLogs", "log");
            modelBuilder.Entity<LinqConsumeErrorLog>().ToTable("ConsumeErrorLogs", "log");
            modelBuilder.Entity<LinqPublishErrorLog>().ToTable("PublishErrorLogs", "log");

        }

    }

    /// <summary>
    /// Logging DbContext için gereken DbSet ve SaveChanges operasyonlarını expose eden adapter arayüzü.
    /// </summary>
    public interface ILinqLoggingDbContextAdapter
    {
        DbSet<LinqEventLog> EventLogs { get; }
        DbSet<LinqHttpCallLog> HttpCallLogs { get; }
        DbSet<LinqConsumeErrorLog> LinqConsumeErrorLogs { get; }
        DbSet<LinqPublishErrorLog> LinqPublishErrorLogs { get; }
        DbSet<LinqSqlLog> SqlLogs { get; }
        DbSet<LinqSqlErrorLog> LinqDatabaseErrorLogs { get; }

        int SaveChanges ();
        Task<int> SaveChangesAsync (CancellationToken cancellationToken = default);
    }
    public static class ModelBuilderExtensions
    {
        public static void ApplyLoggingModel (this ModelBuilder mb, string schema)
        {
            mb.Entity<LinqEventLog>().ToTable("EventLogs", schema);
            mb.Entity<LinqSqlLog>().ToTable("SqlLogs", schema);
            mb.Entity<LinqSqlErrorLog>().ToTable("SqlErrorLogs", schema);
            mb.Entity<LinqConsumeErrorLog>().ToTable("ConsumeErrorLogs", schema);
            mb.Entity<LinqPublishErrorLog>().ToTable("PublishErrorLogs", schema);
            mb.Entity<LinqHttpCallLog>().ToTable("HttpCallLogs", schema);
            mb.Entity<OutboundHttpCallError>().ToTable("HttpCallOutboundErrors", schema);
            mb.Entity<HttpCallInboundError>().ToTable("HttpCallInboundErrors", schema);
        }
    }
}