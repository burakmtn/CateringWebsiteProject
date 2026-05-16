namespace CateringWebsite.Models.ViewModels;

public class NearbyMenuItemViewModel
{
    public MenuItem MenuItem { get; set; } = new();

    public string DistanceText { get; set; } = string.Empty;

    public RatingSummary MenuRating { get; set; } = new();

    public RatingSummary CaretakerRating { get; set; } = new();
}
