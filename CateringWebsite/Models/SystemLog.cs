using System.ComponentModel.DataAnnotations;

namespace CateringWebsite.Models;

public class SystemLog
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [StringLength(40)]
    public string Severity { get; set; } = "Information";

    [Required]
    [StringLength(500)]
    public string Message { get; set; } = string.Empty;

    [StringLength(450)]
    public string? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    [StringLength(256)]
    public string? UserEmail { get; set; }

    [StringLength(80)]
    public string? RelatedEntityType { get; set; }

    [StringLength(80)]
    public string? RelatedEntityId { get; set; }

    [StringLength(80)]
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
