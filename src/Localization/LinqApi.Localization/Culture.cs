namespace LinqApi.Localization
{
    using global::LinqApi.Core;
    using System.ComponentModel.DataAnnotations;

    namespace LinqApi.Localization
    {
        /// <summary>
        /// Represents a culture or language definition.
        /// Inherits from BaseEntity&lt;short&gt; so that the primary key is of type short.
        /// </summary>
        public class Culture : BaseEntity<short>
        {
            /// <summary>
            /// Gets or sets the culture code (e.g. "en-US", "tr-TR").
            /// </summary>
            [Required, MaxLength(10)]
            public string Code { get; set; }

            /// <summary>
            /// Gets or sets the display name of the culture (e.g. "English (United States)").
            /// </summary>
            [Required, MaxLength(100)]
            public string DisplayName { get; set; }

            public virtual ICollection<LinqLocalizationEntity> LocalizationEntities { get; set; }
        }
    }


}