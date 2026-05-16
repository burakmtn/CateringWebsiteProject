namespace CateringWebsite.Services;

public interface IGoogleDistanceService
{
    Task<DistanceMatrixResult> GetDrivingDistanceAsync(string origin, string destination, CancellationToken cancellationToken = default);
}
