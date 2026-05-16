using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models;

public class OrderItemOption
{
    public int Id { get; set; }

    public int OrderItemId { get; set; }

    public OrderItem? OrderItem { get; set; }

    public int? MenuCustomizationOptionId { get; set; }

    public MenuCustomizationOption? MenuCustomizationOption { get; set; }

    [Required]
    [StringLength(80)]
    public string GroupName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    public decimal PriceChange { get; set; }
}
