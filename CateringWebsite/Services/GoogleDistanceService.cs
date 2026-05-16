using System.Text.Json;
using Microsoft.Extensions.Options;

namespace CateringWebsite.Services;

public class GoogleDistanceService : IGoogleDistanceService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleMapsOptions _options;

    public GoogleDistanceService(HttpClient httpClient, IOptions<GoogleMapsOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<DistanceMatrixResult> GetDrivingDistanceAsync(
        string origin,
        string destination,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new DistanceMatrixResult { IsConfigured = false, Status = "API_KEY_MISSING" };
        }

        if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
        {
            return new DistanceMatrixResult { IsConfigured = true, Status = "LOCATION_MISSING" };
        }

        var requestUrl =
            "https://maps.googleapis.com/maps/api/distancematrix/json" +
            $"?units=metric&mode=driving&origins={Uri.EscapeDataString(origin)}" +
            $"&destinations={Uri.EscapeDataString(destination)}&key={Uri.EscapeDataString(_options.ApiKey)}";

        using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new DistanceMatrixResult
            {
                IsConfigured = true,
                Status = response.StatusCode.ToString()
            };
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<DistanceMatrixResponse>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);
        var element = payload?.Rows.FirstOrDefault()?.Elements.FirstOrDefault();

        if (payload?.Status != "OK" || element?.Status != "OK" || element.Distance is null)
        {
            return new DistanceMatrixResult
            {
                IsConfigured = true,
                Status = element?.Status ?? payload?.Status ?? "UNKNOWN"
            };
        }

        return new DistanceMatrixResult
        {
            IsConfigured = true,
            IsSuccess = true,
            DistanceMeters = element.Distance.Value,
            Status = element.Status
        };
    }

    private sealed class DistanceMatrixResponse
    {
        public string Status { get; set; } = string.Empty;

        public List<DistanceMatrixRow> Rows { get; set; } = [];
    }

    private sealed class DistanceMatrixRow
    {
        public List<DistanceMatrixElement> Elements { get; set; } = [];
    }

    private sealed class DistanceMatrixElement
    {
        public string Status { get; set; } = string.Empty;

        public DistanceMatrixValue? Distance { get; set; }
    }

    private sealed class DistanceMatrixValue
    {
        public int Value { get; set; }
    }
}
