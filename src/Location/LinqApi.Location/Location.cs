using LinqApi.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinqApi.Location;
public class Location : BaseEntity<long>
{
    [Required]
    public string PlaceId { get; set; }

    public string? PlaceName { get; set; }
    public string? CategoriesJson { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }
    public string? AddressNumber { get; set; }
    public string? Street { get; set; }
    public string? Municipality { get; set; }

    public string? PostalCode { get; set; }
    public string? SubRegion { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }

    //searchler icin calisiyor!
    public string? NormalizedMunicipality { get; set; }
    public string? NormalizedSubRegion { get; set; }
}