using LinqApi.Core;
using LinqApi.Localization.LinqApi.Localization;

namespace Posonl.Domain
{
    [DisplayProperty("name")]
    public class PosService : BaseViewEntity
    {
        public long PosServiceCategoryId { get; set; }
        public virtual PosServiceCategory PosServiceCategory { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsRegional { get; set; }
        public virtual ICollection<CountryGroup> SupportedCountryGroups { get; set; } // Hangi ülkelerde mevcut
        public virtual ICollection<PosCompany> PosCompanies { get; set; }

        public override string GetLocalizationKeyPrefix() => "PosService";

    }

    [DisplayProperty("name")]
    public class PosServiceCategory : BaseViewEntity
    {
        public virtual ICollection<PosService> PosServices { get; set; }

        public override string GetLocalizationKeyPrefix() => "Category";
    }

}