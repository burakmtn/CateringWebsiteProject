namespace CateringWebsite.Services;

public class CartSessionItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public int MenuItemId { get; set; }

    public int Quantity { get; set; }

    public List<int> SelectedOptionIds { get; set; } = [];
}
