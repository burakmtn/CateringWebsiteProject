namespace CateringWebsite.Services;

public class DistanceMatrixResult
{
    public bool IsConfigured { get; init; }

    public bool IsSuccess { get; init; }

    public int? DistanceMeters { get; init; }

    public string Status { get; init; } = string.Empty;

    public string DistanceText => DistanceMeters is null
        ? string.Empty
        : $"{DistanceMeters.Value / 1000.0:0.0} km";
}
