namespace CateringWebsite.Models.ViewModels;

public class UserDashboardStatsViewModel
{
    public int OrderCount { get; set; }

    public int ReviewCount { get; set; }

    public decimal TotalSpent { get; set; }

    public int NearbyMenuCount { get; set; }
}
