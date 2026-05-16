namespace CateringWebsite.Models.ViewModels;

public class OrderHistoryViewModel
{
    public IReadOnlyList<OrderHistoryItemViewModel> Items { get; set; } = [];
}
