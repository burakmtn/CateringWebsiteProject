namespace CateringWebsite.Models.ViewModels;

public class CartViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; set; } = [];

    public decimal TotalAmount => Items.Sum(item => item.Subtotal);
}
