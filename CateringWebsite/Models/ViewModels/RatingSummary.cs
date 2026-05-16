namespace CateringWebsite.Models.ViewModels;

public class RatingSummary
{
    public double? Average { get; set; }

    public int Count { get; set; }

    public string DisplayText => Average is null ? "No ratings yet" : $"{Average:0.0} ({Count})";
}
