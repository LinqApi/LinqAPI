namespace Posonl.Web.Areas.Dashboard.Models
{
    public class AddCountriesToGroupRequest
    {
        public long CountryGroupId { get; set; }
        public long[] CountryIds { get; set; }
    }


    // Request model for adding supported countries.
    public class AddSupportedCountriesRequest
    {
        public long PosCompanyId { get; set; }
        public long[] CountryIds { get; set; }
    }

    // Request model for removing supported countries.
    public class RemoveSupportedCountriesRequest
    {
        public long PosCompanyId { get; set; }
        public long[] CountryIds { get; set; }
    }
}
