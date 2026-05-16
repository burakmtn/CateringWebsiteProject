namespace CateringWebsite.Services;

public class GoogleMapsOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public double NearbyDistanceKm { get; set; } = 20;
}
