using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    public int MenuItemId { get; set; }

    public MenuItem? MenuItem { get; set; }

    [Required]
    public string CaretakerId { get; set; } = string.Empty;

    public ApplicationUser? Caretaker { get; set; }

    [Required]
    [StringLength(120)]
    public string MenuName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal Subtotal { get; set; }

    public ICollection<OrderItemOption> SelectedOptions { get; set; } = [];

    public OrderItemReview? Review { get; set; }
}
