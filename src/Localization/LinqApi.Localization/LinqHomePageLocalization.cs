using LinqApi.Core;
using LinqApi.Localization.LinqApi.Localization;

namespace LinqApi.Localization
{
    public class HomePage : BaseEntity<long>, ILocalizedEntity<HomePageLocalization>
    {
        // Ana sayfaya ait diğer alanlar (örneğin içerik, resimler vs.) buraya eklenebilir.
        public virtual ICollection<HomePageLocalization> Localizations { get; set; } = new List<HomePageLocalization>();
    }

    public class HomePageLocalization : LinqLocalizationBase
    {
        public long HomePageId { get; set; }
        public HomePage HomePage { get; set; }

        // Burada, HomePage için özel localization key prefix üretim mantığı uygulanır.
        public override string GetLocalizationKeyPrefix() => "HomePage.";
    }

}
