namespace CateringWebsite.Models.ViewModels;

public class OrderConfirmationViewModel
{
    public int OrderId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;
}
