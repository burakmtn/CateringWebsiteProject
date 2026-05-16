using CateringWebsite.Data;
using CateringWebsite.Models;

namespace CateringWebsite.Services;

public class SystemLogService : ISystemLogService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SystemLogService> _logger;

    public SystemLogService(ApplicationDbContext dbContext, ILogger<SystemLogService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogAsync(
        string eventType,
        string message,
        string severity = "Information",
        ApplicationUser? user = null,
        string? userEmail = null,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        string? ipAddress = null)
    {
        try
        {
            _dbContext.SystemLogs.Add(new SystemLog
            {
                EventType = eventType,
                Severity = severity,
                Message = message,
                UserId = user?.Id,
                UserEmail = user?.Email ?? userEmail,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                IpAddress = ipAddress
            });

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "System log could not be stored for event {EventType}.", eventType);
        }
    }
}
