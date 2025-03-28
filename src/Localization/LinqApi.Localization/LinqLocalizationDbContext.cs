using LinqApi.Localization.LinqApi.Localization;
using Microsoft.EntityFrameworkCore;

namespace LinqApi.Localization
{


    /// <summary>
    /// DbContext for localization entries, including localization entities and supported cultures.
    /// Uses the "localization" schema.
    /// </summary>
    public class LinqLocalizationDbContext : DbContext
    {
        private readonly string _schema;

        public LinqLocalizationDbContext(DbContextOptions<LinqLocalizationDbContext> options, string schema)
            : base(options)
        {
            _schema = schema;
        }


        /// <summary>
        /// Gets or sets the localization entries.
        /// </summary>
        public DbSet<LinqLocalizationEntity> LocalizationEntries { get; set; }

        /// <summary>
        /// Gets or sets the supported cultures.
        /// </summary>
        public DbSet<Culture> Cultures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Culture entity
            modelBuilder.Entity<Culture>(entity =>
            {
                entity.ToTable("Cultures", _schema);

                // Use the inherited Id as primary key.
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Code)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(e => e.DisplayName)
                      .HasMaxLength(100)
                      .IsRequired();

                // Seed sample cultures.
                entity.HasData(
                    new Culture { Id = 1, Code = "tr-TR", DisplayName = "Türkçe (Türkiye)" },
                    new Culture { Id = 2, Code = "en-US", DisplayName = "English (United States)" },
                    new Culture { Id = 3, Code = "de-DE", DisplayName = "Deutsch (Deutschland)" }
                );
            });

            // Configure LocalizationEntity and its derived types
            modelBuilder.Entity<LinqLocalizationEntity>(entity =>
            {
                entity.ToTable("LocalizationEntries", _schema);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Culture)
            .WithMany(c => c.LocalizationEntities)
            .HasForeignKey(e => e.CultureId)
            .OnDelete(DeleteBehavior.Restrict);

                // Configure TPH discriminator if needed.
                entity.HasDiscriminator<string>("LocalizationType")
                      .HasValue<LinqHomePageLocalization>("HomePage");
            });
        }
    }

}
