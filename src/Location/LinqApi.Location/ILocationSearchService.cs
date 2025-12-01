using Amazon.LocationService;
using Amazon.LocationService.Model;
using LinqApi.Repository;
using LinqApi.S3;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LinqApi.Location;

public interface ILocationSearchService
{
    Task<IEnumerable<PlaceSuggestionDto>> SearchPlacesAsync(string keyword, CancellationToken cancellationToken = default);
    Task<Location> ResolvePlaceAsync(string placeId, CancellationToken cancellationToken = default);
}

public class LocationSearchService : ILocationSearchService
{
    private readonly ILinqRepository<LocationSearch, long> _searchRepo;
    private readonly ILinqRepository<Location, long> _locationRepo;
    private readonly IAmazonLocationService _locClient;

    private readonly string _placeIndexName;

    // İstanbul koordinatları (longitude, latitude) bias için
    private static readonly List<double> IstanbulBiasPosition = new() { 28.9784, 41.0082 };

    public LocationSearchService (

        IAmazonLocationService locClient,
        IOptions<AwsSettings> awsOptions,
        ILinqRepository<LocationSearch, long> searchRepo,
        ILinqRepository<Location, long> locationRepo)
    {
        _locClient = locClient;

        _placeIndexName = awsOptions.Value.Location.PlaceIndexName;
        _searchRepo = searchRepo;
        _locationRepo = locationRepo;
    }

    public async Task<IEnumerable<PlaceSuggestionDto>> SearchPlacesAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        // 1) DB cache kontrolü
        var (found, cached) = await _searchRepo.TryFindFastAsync(
            ls => ls.Keyword == keyword, cancellationToken);

        if (found && !string.IsNullOrEmpty(cached.Results))
        {
            return JsonSerializer
                .Deserialize<List<PlaceSuggestionDto>>(cached.Results)!
                .AsReadOnly();
        }

        // 2) AWS autocomplete çağrısı
        var awsResponse = await _locClient.SearchPlaceIndexForSuggestionsAsync(
            new SearchPlaceIndexForSuggestionsRequest
            {
                IndexName = _placeIndexName,
                Text = keyword,
                MaxResults = 10,
                BiasPosition = IstanbulBiasPosition
            },
            cancellationToken);

        var suggestions = awsResponse.Results
            .Where(s => !string.IsNullOrEmpty(s.PlaceId) && !string.IsNullOrEmpty(s.Text))
            .Select(s => new PlaceSuggestionDto
            {
                PlaceId = s.PlaceId!,
                Label = s.Text!
            })
            .ToList();

        // 3) Sonuçları DB'de cachele
        var searchEntry = new LocationSearch
        {
            Keyword = keyword,
            Results = JsonSerializer.Serialize(suggestions)
        };
        await _searchRepo.InsertAsync(searchEntry, cancellationToken);
        await _searchRepo.SaveChangesAsync(cancellationToken);

        return suggestions;
    }

    public async Task<Location> ResolvePlaceAsync(
        string placeId,
        CancellationToken cancellationToken = default)
    {
        // 1) Detay zaten DB'de var mı?
        var (found, existing) = await _locationRepo.TryFindFastAsync(
            loc => loc.PlaceId == placeId, cancellationToken);

        if (found && existing != null)
            return existing;

        // 2) Yoksa AWS'den GetPlaceAsync ile detay al
        var response = await _locClient.GetPlaceAsync(new GetPlaceRequest
        {
            IndexName = _placeIndexName,
            PlaceId = placeId
        }, cancellationToken);

        var p = response.Place;

        // 3) Infrastructure.Location modeline çevir ve kaydet
        var location = new Location
        {
            PlaceId = placeId,
            PlaceName = p.Label,
            Latitude = (decimal)p.Geometry.Point[1],
            Longitude = (decimal)p.Geometry.Point[0],
            AddressNumber = p.AddressNumber,
            Street = p.Street,
            Municipality = p.Municipality,
            PostalCode = p.PostalCode,
            SubRegion = p.SubRegion,
            Region = p.Region,
            Country = p.Country
        };

        await _locationRepo.InsertAsync(location, cancellationToken);
        await _locationRepo.SaveChangesAsync(cancellationToken);

        return location;
    }

}
