namespace CateringWebsite.Models.ViewModels;

public class OrderHistoryViewModel
{
    public PagedListViewModel<OrderHistoryItemViewModel> Items { get; set; } = new();
}
