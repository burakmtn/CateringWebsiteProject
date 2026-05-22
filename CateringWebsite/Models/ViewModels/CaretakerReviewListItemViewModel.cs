namespace CateringWebsite.Models.ViewModels;

public class CaretakerReviewListItemViewModel
{
    public int OrderItemId { get; set; }

    public string User { get; set; } = string.Empty;

    public string MenuName { get; set; } = string.Empty;

    public int MenuRating { get; set; }

    public int CaretakerRating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
