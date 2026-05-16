using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models;

public class MenuCustomizationOption
{
    public int Id { get; set; }

    public int MenuItemId { get; set; }

    public MenuItem? MenuItem { get; set; }

    [Required]
    [StringLength(80)]
    public string GroupName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(40)]
    public string OptionType { get; set; } = string.Empty;

    public decimal PriceChange { get; set; }
}
