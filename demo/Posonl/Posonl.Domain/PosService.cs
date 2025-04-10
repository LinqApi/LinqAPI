using LinqApi.Core;
using LinqApi.Localization.LinqApi.Localization;

namespace Posonl.Domain
{
    [DisplayProperty("name")]
    public class PosService : BaseEntity<long>, ILocalizedEntity<PosServiceLocalization>
    {
        // Domain'e ait alanlar:
        public string Name { get; set; }
        public string Description { get; set; }
        public long PosServiceCategoryId { get; set; }
        public virtual PosServiceCategory PosServiceCategory { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsRegional { get; set; }

        // Örneğin, hangi ülkelerde mevcut olduğuna dair alanlar
        public virtual ICollection<CountryGroup>? SupportedCountryGroups { get; set; }
        public virtual ICollection<PosCompany>? PosCompanies { get; set; }

        // Localization kayıtlarını barındıran koleksiyon
        public virtual ICollection<PosServiceLocalization> Localizations { get; set; } = new List<PosServiceLocalization>();
    }

    public class PosServiceLocalization : LinqLocalizationBase
    {
        // Hangi PosService'e ait olduğuna dair yabancı anahtar
        public long PosServiceId { get; set; }
        public virtual PosService PosService { get; set; }

        // Localization key prefix üretimi için gerekli metot
        public override string GetLocalizationKeyPrefix() => "PosService";
    }

    [DisplayProperty("name")]
    public class PosServiceCategory : BaseEntity<long>, ILocalizedEntity<PosServiceCategoryLocalization>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        // Localization kayıtlarını barındıran koleksiyon
        public virtual ICollection<PosServiceCategoryLocalization> Localizations { get; set; } = new List<PosServiceCategoryLocalization>();

        // PosService ile ilişkilendirme (bir kategori, birden fazla servise sahip olabilir)
        public virtual ICollection<PosService>? PosServices { get; set; }
    }


    public class PosServiceCategoryLocalization : LinqLocalizationBase
    {
        // Hangi kategoriye ait olduğuna dair yabancı anahtar
        public long PosServiceCategoryId { get; set; }
        public virtual PosServiceCategory PosServiceCategory { get; set; }

        // Localization key prefix üretimi
        public override string GetLocalizationKeyPrefix() => "Category";
    }

}