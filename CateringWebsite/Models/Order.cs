using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    [StringLength(260)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [StringLength(40)]
    public string Status { get; set; } = "Completed";

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = [];
}
