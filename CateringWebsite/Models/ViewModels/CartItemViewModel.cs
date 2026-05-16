namespace CateringWebsite.Models.ViewModels;

public class CartItemViewModel
{
    public string CartItemId { get; set; } = string.Empty;

    public int MenuItemId { get; set; }

    public string MenuName { get; set; } = string.Empty;

    public string CaretakerName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal OptionsTotal { get; set; }

    public decimal Subtotal { get; set; }

    public IReadOnlyList<string> SelectedOptions { get; set; } = [];
}
