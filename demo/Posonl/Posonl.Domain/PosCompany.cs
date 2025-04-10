using LinqApi.Core;
using LinqApi.Localization.LinqApi.Localization;

namespace Posonl.Domain
{

    [DisplayProperty("name")]
    public class PosCompany : BaseEntity<long>, ILocalizedEntity<PosCompanyLocalization>
    {
        // Temel bilgiler
        public string Name { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }       // Web sitesi URL'si
        public string Headquarters { get; set; }    // Genel merkez (şehir/ülke)
        public int EmployeeCount { get; set; }      // Çalışan sayısı

        // İletişim ve tanıtım bilgileri
        public string LogoUrl { get; set; }         // Logo resmi URL'si
        public string PhoneNumber { get; set; }     // Telefon numarası
        public string Email { get; set; }           // İletişim e-postası
        public string Address { get; set; }         // Fiziksel adres
        

        // Diğer alanlar
        public int FoundedYear { get; set; }
        public string StockTicker { get; set; }

        // İlişkiler
        // Bir POS şirketi birden fazla ülkede aktif olabilir.
        public virtual ICollection<CountryGroup>? CountryGroups { get; set; }

        // Birden fazla hizmet sunabilir.
        public virtual ICollection<PosService>? PosServices { get; set; }

        // Ülkeye özel komisyon oranları
        public virtual ICollection<PosCommissionRate>? CommissionRates { get; set; }

        // Farklı puanlama kategorilerinde aldığı puanlar
        public virtual ICollection<PosCompanyRating>? Ratings { get; set; }

        // Localization kayıtlarını barındıran koleksiyon (her dil için bir kayıt)
        public virtual ICollection<PosCompanyLocalization> Localizations { get; set; } = new List<PosCompanyLocalization>();
    }

    public class PosCompanyLocalization : LinqLocalizationBase
    {
        // Hangi PosCompany'e ait olduğuna dair yabancı anahtar
        public long PosCompanyId { get; set; }
        public virtual PosCompany PosCompany { get; set; }

        // Localization key prefix üretimi: kullanıcı, burada şirkete özel mekanizmayı belirtiyor.
        public override string GetLocalizationKeyPrefix() => "Company";
    }

}