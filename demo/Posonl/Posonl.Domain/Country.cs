using System.ComponentModel.DataAnnotations;

namespace Posonl.Domain
{
    public class Country : BaseEntity
    {
        public string Name { get; set; }          // Örn: Türkiye, Almanya, İngiltere
        public string Code { get; set; }          // İki harfli kod: "tr", "de", "uk"
        public string Currency { get; set; }      // Para birimi: TRY, EUR, GBP
        public string LanguageCode { get; set; }  // "tr-TR", "de-DE", "en-UK"

        // (Opsiyonel) İlgili POS şirketleri – EF Core many-to-many join tablosunu otomatik oluşturur.
        public virtual ICollection<PosCompany>? PosCompanies { get; set; }
        public virtual ICollection<PosService>? PosServices { get; set; }
    }

    public class CountryGroup : BaseEntity
    {

        [MaxLength(100)]
        public string Name { get; set; }

        // Navigation
        public virtual ICollection<PosService> PosServices { get; set; }

    }

    public static class CountryExtensions
    {
        /// <summary>
        /// Verilen ülkeler listesinden Avrupa ülkelerini döndürür.
        /// Bu örnekte, para birimi EUR olan veya ülke kodu belirli Avrupa ülke kodlarından biri ise ülkeyi Avrupa’ya dahil ediyoruz.
        /// </summary>
        public static IEnumerable<Country> GetEuropeanCountries(this IEnumerable<Country> countries)
        {
            // Basit filtre: para birimi EUR veya belirli ülke kodlarına sahip ülkeler
            var europeanCodes = new[]
            {
            "de", "fr", "it", "es", "nl", "be", "se", "at", "dk", "fi", "pt", "ie", "gr", "cz", "pl",
            "ro", "hu", "sk", "si", "bg", "hr", "ee", "lv", "lt", "cy", "lu", "mt", "uk"
        };
            return countries.Where(c =>
                c.Code == "eu" || europeanCodes.Contains(c.Code));
        }

        /// <summary>
        /// Verilen ülkeler listesini direkt döndürür. (Bu metod, okunabilirliği artırmak için yazılmıştır.)
        /// </summary>
        public static IEnumerable<Country> AllCountries(this IEnumerable<Country> countries) => countries;


    }

}