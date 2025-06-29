namespace LinqApi.Localization
{
    using global::LinqApi.Logging;
    using System.ComponentModel.DataAnnotations;

    namespace LinqApi.Localization
    {
        /// <summary>
        /// Represents a localized entity with mandatory Name and Description.
        /// The localization keys are auto-generated as "KeyPrefix.Name" and "KeyPrefix.Description".
        /// </summary>
        public class LinqLocalizationEntity : BaseEntity<long>
        {

            [Required, MaxLength(100)]
            public string Key { get; set; }

            [MaxLength(500)]
            public string Description { get; set; }

            public string Culture { get; set; }

            public string Value { get; set; }
        }
    }
}