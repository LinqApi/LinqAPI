using LinqApi.Epoch;
using LinqApi.Logging.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LinqApi.Logging
{
    public class LinqLoggingDbContext : DbContext, ILinqLoggingDbContextAdapter
    {
        private readonly string _schema;
        private readonly IEpochProvider _epochProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LinqLoggingDbContext(
     DbContextOptions<LinqLoggingDbContext> options,
     IEpochProvider epochProvider,
     IHttpContextAccessor? httpContextAccessor = null
 ) : base(options)
        {
            _epochProvider = epochProvider ?? new DefaultLinqEpochProvider();
            _httpContextAccessor = httpContextAccessor;
            _schema = "log";
        }

        public DbSet<LinqLogEntity> Logs { get; set; }
        public DbSet<LinqEventLog> EventLogs { get; set; }
        public DbSet<LinqHttpCallLog> HttpCallLogs { get; set; }
        public DbSet<LinqConsumeErrorLog> LinqConsumeErrorLogs { get; set; }
        public DbSet<LinqPublishErrorLog> LinqPublishErrorLogs { get; set; }
        public DbSet<LinqSqlLog> SqlLogs { get; set; }
        public DbSet<LinqSqlErrorLog> LinqDatabaseErrorLogs { get; set; }

        private long? GetUserProfileId()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            var claim = user?.FindFirst("userprofile.id")?.Value;
            return long.TryParse(claim, out var id) ? id : null;
        }

        public override int SaveChanges()
        {
            UpdateLogTimestampsAndEpoch();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<LinqLogEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.Now;
                }

                ModifiedDoes(entry);
            }

            UpdateLogTimestampsAndEpoch();
            return await base.SaveChangesAsync(cancellationToken); // eski hali yanlış
        }

        private void ModifiedDoes(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<LinqLogEntity> entry)
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

        private void UpdateLogTimestampsAndEpoch()
        {
            foreach (var entry in ChangeTracker.Entries<LinqLogEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.Now;
                    entry.Entity.Epoch = _epochProvider.GetEpoch(entry.Entity.CreatedAt);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var logEntity = modelBuilder.Entity<LinqLogEntity>();

            // Tablo ve şema ayarı
            logEntity.ToTable("Logs", _schema);

            // CorrelationId'yi primary key olarak ayarlıyoruz ve kolon adını "CorrelationId" yapıyoruz.
            logEntity.HasKey(x => x.Id);

            logEntity.Property(x => x.DurationMs).IsRequired();
            logEntity.Property(x => x.CorrelationId).IsRequired();
            logEntity.Property(x => x.Exception).HasColumnType("nvarchar(max)").IsRequired(false);
            logEntity.Property(x => x.ParentCorrelationId).HasColumnType("varchar(50)").IsRequired(false);
            logEntity.Property(x => x.IsException).IsRequired();
            logEntity.Property(x => x.IsInternal).IsRequired();

            // Gölgeli özellikler
            logEntity.Property<string>("ParentCorrelationId").HasMaxLength(100);
            logEntity.Property<string>("Url").HasMaxLength(500).IsRequired(false);
            logEntity.Property<string>("Method").HasMaxLength(50).IsRequired(false);
            logEntity.Property<string>("RequestBody").HasColumnType("nvarchar(MAX)").IsRequired(false);
            logEntity.Property<string>("ResponseBody").HasColumnType("nvarchar(MAX)").IsRequired(false);
            logEntity.Property<string>("Controller").HasMaxLength(100).IsRequired(false);
            logEntity.Property<string>("Action").HasMaxLength(100).IsRequired(false);
            logEntity.Property<string>("UserAgent").HasMaxLength(500).IsRequired(false);
            logEntity.Property<string>("ClientIP").HasMaxLength(50).IsRequired(false);
            logEntity.Property<string>("QueueName").HasMaxLength(100).IsRequired(false);
            logEntity.Property<string>("OperationName").HasMaxLength(100).IsRequired(false);
            logEntity.Property<string>("AdditionalData").HasMaxLength(500).IsRequired(false);

            logEntity.Property(x => x.CreatedAt).IsRequired();

            // Epoch artık computed column değil, uygulama tarafından set edilecek normal bir property.
            logEntity.Property(x => x.Epoch).IsRequired();


            // Ortak özellikleri tanımla
            modelBuilder.Entity<LinqLogEntity>(logEntity =>
            {
                logEntity.Property(x => x.CreatedAt).IsRequired();
                logEntity.Property(x => x.Epoch).IsRequired();
            });

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
        DbSet<LinqLogEntity> Logs { get; }
        DbSet<LinqEventLog> EventLogs { get; }
        DbSet<LinqHttpCallLog> HttpCallLogs { get; }
        DbSet<LinqConsumeErrorLog> LinqConsumeErrorLogs { get; }
        DbSet<LinqPublishErrorLog> LinqPublishErrorLogs { get; }
        DbSet<LinqSqlLog> SqlLogs { get; }
        DbSet<LinqSqlErrorLog> LinqDatabaseErrorLogs { get; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
    public static class ModelBuilderExtensions
    {
        public static void ApplyLoggingModel(this ModelBuilder mb, string schema)
        {
            mb.HasDefaultSchema(mb.Model.GetDefaultSchema()!);
            var log = mb.Entity<LinqLogEntity>();
            log.ToTable("Logs", schema);

            log.HasKey(x => x.Id);
            log.Property(x => x.DurationMs).IsRequired();
            log.Property(x => x.CorrelationId).IsRequired();
            log.Property(x => x.Exception).HasColumnType("nvarchar(max)").IsRequired(false);
            log.Property(x => x.ParentCorrelationId).HasColumnType("varchar(50)").IsRequired(false);
            log.Property(x => x.IsException).IsRequired();
            log.Property(x => x.IsInternal).IsRequired();

            log.Property<string>("Url").HasMaxLength(500).IsRequired(false);
            log.Property<string>("Method").HasMaxLength(50).IsRequired(false);
            log.Property<string>("RequestBody").HasColumnType("nvarchar(MAX)").IsRequired(false);
            log.Property<string>("ResponseBody").HasColumnType("nvarchar(MAX)").IsRequired(false);

            mb.Entity<LinqLogEntity>().ToTable((string?)null); // base sınıfı tabloya bağlama
            mb.Entity<LinqLogEntity>().Property(x => x.CreatedAt).IsRequired();
            mb.Entity<LinqLogEntity>().Property(x => x.Epoch).IsRequired();

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