namespace LinqApi.Localization
{
    using global::LinqApi.Repository;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Concurrent;
    using System.Text.RegularExpressions;

    namespace LinqApi.Localization.Extensions
    {
        public static class SlugHelper
        {
            public static string Slugify(string text)
            {
                if (string.IsNullOrWhiteSpace(text)) return "";

                var replacements = new Dictionary<char, string>
                {
                    ['ç'] = "c",
                    ['Ç'] = "c",
                    ['ğ'] = "g",
                    ['Ğ'] = "g",
                    ['ı'] = "i",
                    ['İ'] = "i",
                    ['ö'] = "o",
                    ['Ö'] = "o",
                    ['ş'] = "s",
                    ['Ş'] = "s",
                    ['ü'] = "u",
                    ['Ü'] = "u"
                };

                text = string.Concat(text.Select(c => replacements.ContainsKey(c) ? replacements[c] : c.ToString()));
                text = Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9\s-]", "");
                text = Regex.Replace(text, @"\s+", "-").Trim('-');

                return text;
            }

            public static string SlugifyWithId(string text)
            {
                var slug = Slugify(text);
                var shortGuid = Guid.NewGuid().ToString("N")[..6]; // 6 karakterli suffix
                return $"{slug}-{shortGuid}";
            }
        }

        ///// <summary>
        ///// Represents the base entity for all localization entries.
        ///// Inherits from BaseEntity&lt;long&gt;.
        ///// </summary>
        //public abstract class LocalizationEntity : BaseEntity<long>
        //{
        //    /// <summary>
        //    /// Gets or sets the localization name.
        //    /// This value is required and limited to 100 characters.
        //    /// </summary>
        //    [Required, MaxLength(100)]
        //    public string Name { get; set; }

        //    /// <summary>
        //    /// Gets or sets the localization description.
        //    /// This value is optional and limited to 500 characters.
        //    /// </summary>
        //    [MaxLength(500)]
        //    public string Description { get; set; }

        //    /// <summary>
        //    /// Gets or sets the culture code (e.g., "tr-TR", "en-US", "de-DE").
        //    /// </summary>
        //    [Required, MaxLength(10)]
        //    public string Culture { get; set; }

        //    /// <summary>
        //    /// Returns the localization key prefix used for resource lookup.
        //    /// By default, it returns the type name plus a dot (e.g. "LinqHomePageLocalization.").
        //    /// </summary>
        //    public virtual string GetLocalizationKeyPrefix() => GetType().Name + ".";
        //}

        /// <summary>
        /// A concrete localization entity representing homepage localization.
        /// </summary>
        public class LinqHomePageLocalization : LocalizationEntity
        {
            // You can add additional properties specific to the homepage localization here.
        }

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

                    // Seed sample data for demonstration.
                    entity.HasData(
                        new LinqHomePageLocalization
                        {
                            Id = 1,
                            Name = "Ana Sayfa", // Turkish
                            Description = "Ana sayfa için varsayılan yerelleştirme",
                            Culture = "tr-TR"
                        },
                        new LinqHomePageLocalization
                        {
                            Id = 2,
                            Name = "Home Page", // English
                            Description = "Default localization for home page",
                            Culture = "en-US"
                        },
                        new LinqHomePageLocalization
                        {
                            Id = 3,
                            Name = "Startseite", // German
                            Description = "Standardlokalisierung für die Startseite",
                            Culture = "de-DE"
                        }
                    );
                });
            }
        }

        /// <summary>
        /// Provides methods for retrieving localized strings based on keys.
        /// </summary>
        public interface ILocalizationProvider
        {
            /// <summary>
            /// Retrieves the localized string corresponding to the given key.
            /// </summary>
            /// <param name="key">The localization key.</param>
            /// <param name="cancellationToken">A cancellation token.</param>
            /// <returns>The localized string, or null if not found.</returns>
            Task<string?> GetLocalizedValueAsync(string key, CancellationToken cancellationToken);
        }

        /// <summary>
        /// The default implementation of <see cref="ILocalizationProvider"/> that first checks an in-memory cache
        /// and then retrieves entries from the localization repository.
        /// </summary>
        public class DefaultLinqLocalizationProvider : ILocalizationProvider
        {
            private readonly ILocalizationRepository _repository;
            private readonly ConcurrentDictionary<string, string> _cache = new();

            public DefaultLinqLocalizationProvider(ILocalizationRepository repository)
            {
                _repository = repository;
            }

            public async Task<string?> GetLocalizedValueAsync(string key, CancellationToken cancellationToken)
            {
                if (_cache.TryGetValue(key, out string value))
                {
                    return value;
                }

                // Load all localization entries (you might want a more efficient lookup in a real implementation)
                var entries = await _repository.GetAllAsync(cancellationToken);
                foreach (var entry in entries)
                {
                    // Assume GetLocalizationKeyPrefix() + "Name" for example is the key.
                    var localizedKey = entry.GetLocalizationKeyPrefix() + "Name";
                    if (!_cache.ContainsKey(localizedKey))
                    {
                        _cache.TryAdd(localizedKey, entry.Name);
                    }
                }

                _cache.TryGetValue(key, out value);
                return value;
            }
        }
    }
}