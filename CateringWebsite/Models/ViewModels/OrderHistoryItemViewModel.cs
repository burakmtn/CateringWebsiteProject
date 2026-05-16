namespace CateringWebsite.Models.ViewModels;

public class OrderHistoryItemViewModel
{
    public int OrderId { get; set; }

    public int OrderItemId { get; set; }

    public string MenuName { get; set; } = string.Empty;

    public string CaretakerName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Subtotal { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool CanReview { get; set; }

    public bool HasReview { get; set; }
}
