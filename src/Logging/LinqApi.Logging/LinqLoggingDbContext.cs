using LinqApi.Core.Log;
using LinqApi.Epoch;
using Microsoft.EntityFrameworkCore;

namespace LinqApi.Logging
{
    public class LinqLoggingDbContext : DbContext
    {
        private readonly string _schema;
        private readonly IEpochProvider _epochProvider;

        public LinqLoggingDbContext(DbContextOptions<LinqLoggingDbContext> options, IEpochProvider epochProvider)
            : base(options)
        {
            _schema = "log";
            _epochProvider = epochProvider ?? new DefaultLinqEpochProvider();
        }

        public DbSet<LinqLogEntity> Logs { get; set; }
        public DbSet<LinqEventLog> EventLogs { get; set; }
        public DbSet<LinqHttpCallLog> HttpCallLogs { get; set; }
        public DbSet<LinqConsumeErrorLog> LinqConsumeErrorLogs { get; set; }
        public DbSet<LinqPublishErrorLog> LinqPublishErrorLogs { get; set; }
        public DbSet<LinqSqlLog> SqlLogs { get; set; }
        public DbSet<LinqSqlErrorLog> LinqDatabaseErrorLogs { get; set; }

        public override int SaveChanges()
        {
            UpdateEpochValues();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateEpochValues();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateEpochValues()
        {
            foreach (var entry in ChangeTracker.Entries<LinqLogEntity>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    // Her kayıtta, CreatedAt'e göre epoch hesapla.
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

            //logEntity.Property(x => x.Id).HasMaxLength(50).IsRequired();
            logEntity.Property(x => x.DurationMs).IsRequired();
            logEntity.Property(x => x.CorrelationId).IsRequired();
            logEntity.Property(x => x.Exception).HasColumnType("nvarchar(max)");
            logEntity.Property(x => x.ParentCorrelationId).HasColumnType("varchar(50)");
            logEntity.Property(x => x.IsException).IsRequired();
            logEntity.Property(x => x.IsInternal).IsRequired();
            logEntity.Property(x => x.LogType).IsRequired();

            // Gölgeli özellikler
            logEntity.Property<string>("ParentCorrelationId").HasMaxLength(100);
            logEntity.Property<string>("Url").HasMaxLength(500);
            logEntity.Property<string>("Method").HasMaxLength(50);
            logEntity.Property<string>("RequestBody").HasColumnType("nvarchar(max)");
            logEntity.Property<string>("ResponseBody").HasColumnType("nvarchar(max)");
            logEntity.Property<string>("Controller").HasMaxLength(100);
            logEntity.Property<string>("Action").HasMaxLength(100);
            logEntity.Property<string>("UserAgent").HasMaxLength(500);
            logEntity.Property<string>("ClientIP").HasMaxLength(50);
            logEntity.Property<string>("QueueName").HasMaxLength(100);
            logEntity.Property<string>("OperationName").HasMaxLength(100);
            logEntity.Property<string>("AdditionalData").HasMaxLength(500);

            logEntity.Property(x => x.CreatedAt).IsRequired();

            // Epoch artık computed column değil, uygulama tarafından set edilecek normal bir property.
            logEntity.Property(x => x.Epoch).IsRequired();

            //logEntity.HasIndex("Epoch").
            //         .IncludeProperties("DurationMs", "CorrelationId", "LogType", "ParentCorrelationId", "Url", "Method", "IsInternal");

            // TPH yapılandırması: Discriminator "LogType"
            logEntity.HasDiscriminator<string>("LogType")
    .HasValue<LinqEventLog>("Event")
    .HasValue<LinqHttpCallLog>("HttpCall")
    .HasValue<LinqSqlLog>("Database")
    .HasValue<LinqSqlErrorLog>("DatabaseError")
    .HasValue<LinqConsumeErrorLog>("ConsumeError")
    .HasValue<LinqPublishErrorLog>("PublishError");
        }
    }
}