using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CateringWebsite.Models.ViewModels;

public class MenuItemFormViewModel
{
    public int? Id { get; set; }

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

    [Display(Name = "Menu image")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImagePath { get; set; }

    [Display(Name = "Removable ingredients")]
    public string? RemovableIngredientsText { get; set; }

    [Display(Name = "Optional additions")]
    public string? OptionalAdditionsText { get; set; }

    [Display(Name = "Option groups")]
    public string? OptionGroupsText { get; set; }
}
