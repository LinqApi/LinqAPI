namespace Posonl.Web.Areas.Dashboard.Models
{
    public class RemoveCountriesFromGroupRequest
    {
        public long CountryGroupId { get; set; }
        public long[] CountryIds { get; set; }
    }
}
