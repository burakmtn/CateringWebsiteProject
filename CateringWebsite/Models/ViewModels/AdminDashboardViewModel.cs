namespace CateringWebsite.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int UserCount { get; set; }

    public int CaretakerCount { get; set; }

    public int MenuItemCount { get; set; }

    public int OrderCount { get; set; }

    public int ReviewCount { get; set; }

    public int LogCount { get; set; }

    public decimal TotalOrderAmount { get; set; }
}
