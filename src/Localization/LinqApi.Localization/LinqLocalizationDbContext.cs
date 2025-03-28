using LinqApi.Localization.LinqApi.Localization;
using Microsoft.EntityFrameworkCore;

namespace LinqApi.Localization
{


    /// <summary>
    /// Represents the Entity Framework Core DbContext for localization entries.
    /// This context uses Table-per-Hierarchy (TPH) mapping with a discriminator column "LocalizationType"
    /// to differentiate between various localization entities.
    /// </summary>
    public class LinqLocalizationDbContext : DbContext
    {
        private readonly string _schema = "localization";

        /// <summary>
        /// Initializes a new instance of the <see cref="LinqLocalizationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public LinqLocalizationDbContext(DbContextOptions<LinqLocalizationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the localization entries.
        /// All entities inheriting from <see cref="LocalizationEntity"/> are stored in this set.
        /// </summary>
        public DbSet<LocalizationEntity> LocalizationEntries { get; set; }

        /// <summary>
        /// Configures the model for the localization DbContext.
        /// Uses a discriminator column "LocalizationType" to differentiate between localization types.
        /// Seeds sample localization entries for demonstration.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map all LocalizationEntity-derived types to a single table "LocalizationEntries" in the "localization" schema.
            modelBuilder.Entity<LocalizationEntity>(entity =>
            {
                entity.ToTable("LocalizationEntries", _schema);
                // Set primary key (inherited from BaseEntity<long>)
                entity.HasKey(e => e.Id);
                // Common properties
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Culture).HasMaxLength(10).IsRequired();

                // Use a discriminator to differentiate localization types.
                entity.HasDiscriminator<string>("LocalizationType")
                      .HasValue<LinqHomePageLocalization>("HomePage");
            });
        }
    }
}
