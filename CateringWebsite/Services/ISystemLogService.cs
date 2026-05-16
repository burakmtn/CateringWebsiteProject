using CateringWebsite.Models;

namespace CateringWebsite.Services;

public interface ISystemLogService
{
    Task LogAsync(
        string eventType,
        string message,
        string severity = "Information",
        ApplicationUser? user = null,
        string? userEmail = null,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? ipAddress = null);
}
