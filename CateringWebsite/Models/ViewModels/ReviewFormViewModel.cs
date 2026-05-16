using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models.ViewModels;

public class ReviewFormViewModel
{
    public int OrderItemId { get; set; }

    public string MenuName { get; set; } = string.Empty;

    public string CaretakerName { get; set; } = string.Empty;

    [Range(1, 5)]
    [Display(Name = "Menu rating")]
    public int MenuRating { get; set; } = 5;

    [Range(1, 5)]
    [Display(Name = "Caretaker rating")]
    public int CaretakerRating { get; set; } = 5;

    [StringLength(800)]
    public string Comment { get; set; } = string.Empty;
}
