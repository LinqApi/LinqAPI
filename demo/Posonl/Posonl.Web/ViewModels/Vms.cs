namespace Posonl.Web.ViewModels
{

    #region ViewModels

    public class CreateCountryVm
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Currency { get; set; }
        public string LanguageCode { get; set; }
    }

    public class UpdateCountryVm : CreateCountryVm
    {
        public long Id { get; set; }
    }

    public class CreateCountryGroupVm
    {
        public string Name { get; set; }
    }

    public class UpdateCountryGroupVm : CreateCountryGroupVm
    {
        public long Id { get; set; }
    }

    public class CreatePosCommissionRateVm
    {
        public long PosCompanyId { get; set; }
        public long CountryId { get; set; }
        public decimal CommissionPercentage { get; set; }
    }

    public class UpdatePosCommissionRateVm : CreatePosCommissionRateVm
    {
        public long Id { get; set; }
    }

    public class CreatePosCompanyVm
    {
        public string Website { get; set; }
        public string Headquarters { get; set; }
        public int EmployeeCount { get; set; }
        public string LogoUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int FoundedYear { get; set; }
        public string StockTicker { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdatePosCompanyVm : CreatePosCompanyVm
    {
        public long Id { get; set; }
    }

    public class CreatePosCompanyTypeVm
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdatePosCompanyTypeVm : CreatePosCompanyTypeVm
    {
        public long Id { get; set; }
    }

    public class CreatePosCompanyDescriptionVm
    {
        public int PosCompanyId { get; set; }
        public int FoundedYear { get; set; }
        public string StockTicker { get; set; }
        public int HeadquartersCountryId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
    }

    public class UpdatePosCompanyDescriptionVm : CreatePosCompanyDescriptionVm
    {
        public long Id { get; set; }
    }

    public class CreatePosCompanyRatingVm
    {
        public long PosCompanyId { get; set; }
        public long RatingCategoryId { get; set; }
        public decimal Score { get; set; }
    }

    public class UpdatePosCompanyRatingVm : CreatePosCompanyRatingVm
    {
        public long Id { get; set; }
    }

    public class CreatePosServiceVm
    {
        public long PosServiceCategoryId { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsRegional { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdatePosServiceVm : CreatePosServiceVm
    {
        public long Id { get; set; }
    }

    public class CreatePosServiceCategoryVm
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdatePosServiceCategoryVm : CreatePosServiceCategoryVm
    {
        public long Id { get; set; }
    }

    public class CreateRatingCategoryVm
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class UpdateRatingCategoryVm : CreateRatingCategoryVm
    {
        public long Id { get; set; }
    }

    #endregion
}
