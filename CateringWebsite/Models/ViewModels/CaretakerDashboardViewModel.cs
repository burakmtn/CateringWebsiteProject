namespace CateringWebsite.Models.ViewModels;

public class CaretakerDashboardViewModel
{
    public PagedListViewModel<MenuItem> MenuItems { get; set; } = new();

    public int MenuItemCount { get; set; }

    public int ReceivedOrderCount { get; set; }

    public int CompletedOrderCount { get; set; }

    public decimal TotalRevenue { get; set; }

    public double? AverageCaretakerRating { get; set; }

    public int ReviewCount { get; set; }

    public PagedListViewModel<CaretakerReviewListItemViewModel> Reviews { get; set; } = new();

    public bool LocationMissing { get; set; }
}
