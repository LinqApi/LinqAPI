namespace LinqApi.Localization
{
    namespace LinqApi.Localization.Extensions
    {
        /// <summary>
        /// Represents configuration options for the LinqApi localization system.
        /// </summary>
        public class LinqLocalizationOptions
        {
            /// <summary>
            /// Gets or sets the default culture to use if none is specified.
            /// </summary>
            public string DefaultCulture { get; set; } = "en-US";

            /// <summary>
            /// Gets or sets a flag indicating whether the localization data should be cached in memory.
            /// </summary>
            public bool EnableCaching { get; set; } = true;
        }
    }

}
