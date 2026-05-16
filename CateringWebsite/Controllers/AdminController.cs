using CateringWebsite.Data;
using CateringWebsite.Models.ViewModels;
using CateringWebsite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringWebsite.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISystemLogService _systemLogService;

    public AdminController(ApplicationDbContext dbContext, ISystemLogService systemLogService)
    {
        _dbContext = dbContext;
        _systemLogService = systemLogService;
    }

    public async Task<IActionResult> Index()
    {
        await _systemLogService.LogAsync(
            "AdminAction",
            "Admin dashboard viewed.",
            userEmail: User.Identity?.Name,
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());
        return View();
    }

    public async Task<IActionResult> Logs()
    {
        await _systemLogService.LogAsync(
            "AdminAction",
            "Admin logging page viewed.",
            userEmail: User.Identity?.Name,
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        var logs = await _dbContext.SystemLogs
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAt)
            .Take(200)
            .Select(log => new SystemLogListItemViewModel
            {
                CreatedAt = log.CreatedAt,
                EventType = log.EventType,
                Severity = log.Severity,
                Message = log.Message,
                UserEmail = log.UserEmail,
                RelatedEntityType = log.RelatedEntityType,
                RelatedEntityId = log.RelatedEntityId,
                IpAddress = log.IpAddress
            })
            .ToListAsync();

        return View(new SystemLogListViewModel { Logs = logs });
    }
}
