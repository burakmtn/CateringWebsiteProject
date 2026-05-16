namespace CateringWebsite.Models.ViewModels;

public class NearbyMenusViewModel
{
    public string? UserLocationAddress { get; set; }

    public double NearbyDistanceKm { get; set; }

    public bool GoogleMapsConfigured { get; set; }

    public string? StatusMessage { get; set; }

    public UserDashboardStatsViewModel Stats { get; set; } = new();

    public IReadOnlyList<NearbyMenuItemViewModel> Menus { get; set; } = [];
}
