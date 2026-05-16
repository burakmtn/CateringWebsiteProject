using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models.ViewModels;

public class AddToCartViewModel
{
    public int MenuItemId { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;

    public List<int> SelectedOptionIds { get; set; } = [];
}
