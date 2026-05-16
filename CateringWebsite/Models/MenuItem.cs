using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models;

public class MenuItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string CaretakerId { get; set; } = string.Empty;

    public ApplicationUser? Caretaker { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
