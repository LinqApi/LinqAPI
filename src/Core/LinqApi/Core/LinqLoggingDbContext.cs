using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LinqApi.Model;
using System;

namespace LinqApi.Core
{
    /// <summary>
    /// The unified logging DbContext.
    /// Uses table-per-hierarchy (TPH) mapping to store all logs in a single table named "Logs"
    /// in the specified schema (here, "log"). All log entries (HTTP, API, MassTransit, SQL, etc.)
    /// are stored as BaseLogEntity (or its derived types) and distinguished by the LogType discriminator.
    /// It also defines a computed column "Epoch" for efficient time-based queries.
    /// </summary>
    public class LinqLoggingDbContext : DbContext
    {
        private readonly string _schema;

        /// <summary>
        /// Constructs a new instance using DI.
        /// The schema is fixed to "log" here.
        /// </summary>
        public LinqLoggingDbContext(DbContextOptions<LinqLoggingDbContext> options)
            : base(options)
        {
            _schema = "log";
        }

        /// <summary>
        /// (Optional) Additional constructor for design-time usage where you may pass a schema.
        /// </summary>
        public LinqLoggingDbContext(DbContextOptions<LinqLoggingDbContext> options, string schema)
            : base(options)
        {
            _schema = schema;
        }

        /// <summary>
        /// Unified DbSet for all log entries.
        /// Use TPH mapping (with a discriminator) to query across all logs.
        /// </summary>
        public DbSet<BaseLogEntity> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureBaseLogEntity(modelBuilder.Entity<BaseLogEntity>());
        }

        /// <summary>
        /// Configures the unified log entity using TPH mapping.
        /// </summary>
        /// <param name="entity">The entity type builder for BaseLogEntity.</param>
        private void ConfigureBaseLogEntity(EntityTypeBuilder<BaseLogEntity> entity)
        {
            // Map all log entries to a single table "Logs" in the designated schema.
            entity.ToTable("Logs", _schema);

            // Primary key and base properties.
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(x => x.CorrelationId)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(x => x.DurationMs)
                  .IsRequired();

            entity.Property(x => x.Exception)
                  .HasColumnType("nvarchar(max)");

            entity.Property(x => x.IsException)
                  .IsRequired();

            entity.Property(x => x.IsInternal)
                  .IsRequired();

            // The LogType property acts as a discriminator.
            entity.Property(x => x.LogType)
                  .IsRequired();

            // Shadow properties for HTTP-specific fields.
            entity.Property<string>("ParentCorrelationId")
                  .HasMaxLength(100);
            entity.Property<string>("Url")
                  .HasMaxLength(500);
            entity.Property<string>("Method")
                  .HasMaxLength(50);
            entity.Property<string>("RequestBody")
                  .HasColumnType("nvarchar(max)");
            entity.Property<string>("ResponseBody")
                  .HasColumnType("nvarchar(max)");
            entity.Property<string>("Controller")
                  .HasMaxLength(100);
            entity.Property<string>("Action")
                  .HasMaxLength(100);
            entity.Property<string>("UserAgent")
                  .HasMaxLength(500);
            entity.Property<string>("ClientIP")
                  .HasMaxLength(50);

            // Shadow properties for event logs.
            entity.Property<string>("QueueName")
                  .HasMaxLength(100);
            entity.Property<string>("OperationName")
                  .HasMaxLength(100);
            entity.Property<string>("AdditionalData")
                  .HasMaxLength(500);

            // All logs have a CreatedAt timestamp.
            entity.Property(x => x.CreatedAt)
                  .IsRequired();

            // Computed column for Epoch (number of 4-minute intervals since Unix epoch).
            entity.Property<long>("Epoch")
                  .HasComputedColumnSql("DATEDIFF(MILLISECOND, '1970-01-01', [CreatedAt]) / 240000", stored: true);

            // Create a covering index on Epoch including several key properties.
            entity.HasIndex("Epoch")
                  .IncludeProperties(e => new
                  {
                      e.DurationMs,
                      e.CorrelationId,
                      e.LogType,
                      ParentCorrelationId = EF.Property<string>(e, "ParentCorrelationId"),
                      Url = EF.Property<string>(e, "Url"),
                      Method = EF.Property<string>(e, "Method"),
                      e.IsInternal,
                      e.Exception
                  });

            // Configure TPH discriminator column if needed (using LogType as int).
            entity.HasDiscriminator<int>("LogType")
                  .HasValue<BaseLogEntity>((int)LogType.Info);
        }
    }
}
