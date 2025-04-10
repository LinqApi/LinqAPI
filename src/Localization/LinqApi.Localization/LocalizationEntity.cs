namespace LinqApi.Localization
{
    using global::LinqApi.Core;
    using System.ComponentModel.DataAnnotations;

    namespace LinqApi.Localization
    {
        /// <summary>
        /// Represents a localized entity with mandatory Name and Description.
        /// The localization keys are auto-generated as "KeyPrefix.Name" and "KeyPrefix.Description".
        /// </summary>
        public abstract class LinqLocalizationEntity : BaseEntity<long>
        {
            [Required, MaxLength(100)]
            public string Name { get; set; }

            [MaxLength(500)]
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the foreign key referencing the culture for this localization entry.
            /// </summary>
            public short CultureId { get; set; }

            /// <summary>
            /// Navigation property for the associated culture.
            /// </summary>
            public Culture Culture { get; set; }

            /// <summary>
            /// Returns the localization key prefix for this entity.
            /// For example, for PosService it might return "PosService.", for PosCompany "PosCompany.".
            /// </summary>
            public virtual string GetLocalizationKeyPrefix() => GetType().Name + ".";
        }

        public abstract class BaseViewEntity : LinqLocalizationEntity
        {
            public string? Slug { get; set; }
            public string? Title { get; set; }
            public string? MetaDescription { get; set; }
            public string? MetaKeywords { get; set; }
            public bool IsPublished { get; set; } = true;
        }

        public interface ILocalizedEntity<TLocalization>
        {
            ICollection<TLocalization> Localizations { get; set; }
        }

        public abstract class LinqLocalizationBase : BaseEntity<long>
        {
            [Required, MaxLength(100)]
            public string Name { get; set; }

            [MaxLength(500)]
            public string Description { get; set; }

            // SEO ve view ile ilgili alanlar
            public string? Slug { get; set; }
            public string? Title { get; set; }
            public string? MetaDescription { get; set; }
            public string? MetaKeywords { get; set; }
            public bool IsPublished { get; set; } = true;

            // Culture ilişkisi (zorunlu alan)
            public short CultureId { get; set; }
            public Culture Culture { get; set; }

            /// <summary>
            /// Localization key prefix’i üretir.
            /// Geliştirici, kendi entity’sine özel mantığı uygulaması için bu metodu override edebilir.
            /// </summary>
            public abstract string GetLocalizationKeyPrefix();
        }


    }


}