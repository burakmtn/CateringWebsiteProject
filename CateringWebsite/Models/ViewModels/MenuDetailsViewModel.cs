namespace CateringWebsite.Models.ViewModels;

public class MenuDetailsViewModel
{
    public MenuItem MenuItem { get; set; } = new();

    public string DistanceText { get; set; } = string.Empty;
}
