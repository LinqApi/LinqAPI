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
            [Required, MaxLength(10)]
            public string Code { get; set; }

            [Required, MaxLength(100)]
            public string DisplayName { get; set; }

            // Eğer isterseniz, ilişkisel navigasyon ekleyebilirsiniz, fakat 
            // localizable entity’lerimiz artık her biri kendi tablosunda olacak.
            // public virtual ICollection<HomePageLocalization> HomePageLocalizations { get; set; }
        }
    }


}