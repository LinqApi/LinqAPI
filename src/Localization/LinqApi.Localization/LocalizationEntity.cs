namespace LinqApi.Localization
{
    using global::LinqApi.Core;
    using System.ComponentModel.DataAnnotations;

    namespace LinqApi.Localization
    {
        public abstract class LocalizationEntity : BaseEntity<long>
        {
            // Her entity için zorunlu Name ve Description alanı,
            // bunlar otomatik olarak "KeyPrefix.Name" ve "KeyPrefix.Description" şeklinde resx'ten çekilebilir.
            [Required, MaxLength(100)]
            public string Name { get; set; }

            [MaxLength(500)]
            public string Description { get; set; }
            public string Culture { get;  set; }

            /// <summary>
            /// Bu entity'ye ait yerelleştirme key prefix'ini döndürür.
            /// Örneğin, PosService için "PosService.", PosCompany için "PosCompany." gibi.
            /// </summary>
            public virtual string GetLocalizationKeyPrefix() => GetType().Name + ".";
        }
    }
}