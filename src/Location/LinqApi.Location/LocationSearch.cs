using LinqApi.Core;

namespace LinqApi.Location;
public class LocationSearch : BaseEntity<long>
{
    public string Keyword { get; set; }

    public string Results { get; set; }
}