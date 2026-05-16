using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models.ViewModels;

public class MenuItemFormViewModel
{
    [Required]
    [StringLength(120)]
    [Display(Name = "Menu name")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(500)]
    [Display(Name = "Short description")]
    public string Description { get; set; } = string.Empty;
}
