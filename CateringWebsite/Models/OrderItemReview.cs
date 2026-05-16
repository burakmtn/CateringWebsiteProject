using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models;

public class OrderItemReview
{
    public int Id { get; set; }

    public int OrderItemId { get; set; }

    public OrderItem? OrderItem { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public int MenuItemId { get; set; }

    public MenuItem? MenuItem { get; set; }

    [Required]
    public string CaretakerId { get; set; } = string.Empty;

    public ApplicationUser? Caretaker { get; set; }

    [Range(1, 5)]
    public int MenuRating { get; set; }

    [Range(1, 5)]
    public int CaretakerRating { get; set; }

    [StringLength(800)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
