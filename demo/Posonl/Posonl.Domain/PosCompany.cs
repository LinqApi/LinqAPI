using LinqApi.Localization.LinqApi.Localization;

namespace Posonl.Domain
{
    public class News : BaseViewEntity
    {
        public string ContentHtml { get; set; }
    }
    public class PosCompany : BaseViewEntity
    {
        // Temel bilgiler

        public string Website { get; set; }        // Web sitesi URL'si
        public string Headquarters { get; set; }   // Genel merkez (şehir/ülke)
        public int EmployeeCount { get; set; }     // Çalışan sayısı

        // İletişim ve tanıtım bilgileri
        public string LogoUrl { get; set; }        // Logo resmi URL'si
        public string PhoneNumber { get; set; }    // Telefon numarası
        public string Email { get; set; }          // İletişim e-postası
        public string Address { get; set; }        // Fiziksel adres

        // İlişkiler

        // Bir POS şirketi birden fazla ülkede aktif olabilir.
        public virtual ICollection<Country> SupportedCountries { get; set; }

        // Birden fazla hizmet sunabilir.
        public virtual ICollection<PosService> PosServices { get; set; }

        // Ülkeye özel komisyon oranları
        public virtual ICollection<PosCommissionRate> CommissionRates { get; set; }

        // Farklı puanlama kategorilerinde aldığı puanlar
        public virtual ICollection<PosCompanyRating> Ratings { get; set; }
        public int FoundedYear { get; set; }
        public string StockTicker { get; set; }


        public override string GetLocalizationKeyPrefix() => "Company";
    }

    public class PosCompanyType : BaseViewEntity
    {
        public override string GetLocalizationKeyPrefix() => "PosCompanyType";
    }

    public class PosCompanyDescription : BaseViewEntity
    {
        public int PosCompanyId { get; set; } // Foreign key to PosCompany
        public int FoundedYear { get; set; }
        public string StockTicker { get; set; }
        public int HeadquartersCountryId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public override string GetLocalizationKeyPrefix() => "PosCompany";
    }

}