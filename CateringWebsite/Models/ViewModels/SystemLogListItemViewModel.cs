namespace CateringWebsite.Models.ViewModels;

public class SystemLogListItemViewModel
{
    public DateTime CreatedAt { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? UserEmail { get; set; }

    public string? RelatedEntityType { get; set; }

    public string? RelatedEntityId { get; set; }

    public string? IpAddress { get; set; }
}
