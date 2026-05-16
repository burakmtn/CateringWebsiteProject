namespace CateringWebsite.Models.ViewModels;

public class AdminOrderListItemViewModel
{
    public int OrderId { get; set; }

    public string Customer { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int ItemCount { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}
