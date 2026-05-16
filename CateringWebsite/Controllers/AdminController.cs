using CateringWebsite.Data;
using CateringWebsite.Models;
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

    public async Task<IActionResult> Logs(string search = "", int page = 1)
    {
        await _systemLogService.LogAsync(
            "AdminAction",
            "Admin logging page viewed.",
            userEmail: User.Identity?.Name,
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        return View(new SystemLogListViewModel
        {
            Logs = await GetLogsAsync(search, page)
        });
    }

    public async Task<IActionResult> Users(string search = "", int page = 1)
    {
        await LogAdminTableViewAsync("Admin users table viewed.");
        return View(await GetUsersAsync(null, search, page));
    }

    public async Task<IActionResult> Caterers(string search = "", int page = 1)
    {
        await LogAdminTableViewAsync("Admin caterers table viewed.");
        return View(await GetUsersAsync(AppRoles.Caretaker, search, page));
    }

    public async Task<IActionResult> Orders(string search = "", int page = 1)
    {
        await LogAdminTableViewAsync("Admin orders table viewed.");
        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .Include(order => order.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(order =>
                order.Id.ToString().Contains(search) ||
                order.Status.Contains(search) ||
                order.User!.Email!.Contains(search) ||
                order.User.DisplayName!.Contains(search));
        }

        return View(await ToPagedListAsync(
            query.OrderByDescending(order => order.CreatedAt),
            order => new AdminOrderListItemViewModel
            {
                OrderId = order.Id,
                Customer = DisplayUser(order.User),
                Status = order.Status,
                ItemCount = order.Items.Count,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt
            },
            search,
            page));
    }

    public async Task<IActionResult> Ratings(string search = "", int page = 1)
    {
        await LogAdminTableViewAsync("Admin ratings table viewed.");
        var query = _dbContext.OrderItemReviews
            .AsNoTracking()
            .Include(review => review.User)
            .Include(review => review.MenuItem)
            .Include(review => review.Caretaker)
            .Include(review => review.OrderItem)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(review =>
                review.Comment.Contains(search) ||
                review.User!.Email!.Contains(search) ||
                review.MenuItem!.Name.Contains(search) ||
                review.Caretaker!.Email!.Contains(search));
        }

        return View(await ToPagedListAsync(
            query.OrderByDescending(review => review.CreatedAt),
            review => new AdminRatingListItemViewModel
            {
                Id = review.Id,
                OrderItemId = review.OrderItemId,
                User = DisplayUser(review.User),
                MenuName = review.MenuItem != null ? review.MenuItem.Name : review.OrderItem!.MenuName,
                Caretaker = DisplayUser(review.Caretaker),
                MenuRating = review.MenuRating,
                CaretakerRating = review.CaretakerRating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            },
            search,
            page));
    }

    private async Task<PagedListViewModel<SystemLogListItemViewModel>> GetLogsAsync(string search = "", int page = 1)
    {
        var query = _dbContext.SystemLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(log =>
                log.EventType.Contains(search) ||
                log.Severity.Contains(search) ||
                log.Message.Contains(search) ||
                log.UserEmail!.Contains(search) ||
                log.RelatedEntityType!.Contains(search));
        }

        return await ToPagedListAsync(
            query.OrderByDescending(log => log.CreatedAt),
            log => new SystemLogListItemViewModel
            {
                CreatedAt = log.CreatedAt,
                EventType = log.EventType,
                Severity = log.Severity,
                Message = log.Message,
                UserEmail = log.UserEmail,
                RelatedEntityType = log.RelatedEntityType,
                RelatedEntityId = log.RelatedEntityId,
                IpAddress = log.IpAddress
            },
            search,
            page);
    }

    private async Task<PagedListViewModel<AdminUserListItemViewModel>> GetUsersAsync(string? roleName, string search, int page)
    {
        var query = _dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            var roleId = await _dbContext.Roles
                .Where(role => role.Name == roleName)
                .Select(role => role.Id)
                .FirstOrDefaultAsync();

            query = query.Where(user => _dbContext.UserRoles.Any(role => role.UserId == user.Id && role.RoleId == roleId));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(user =>
                user.Email!.Contains(search) ||
                user.DisplayName!.Contains(search) ||
                user.LocationAddress!.Contains(search));
        }

        var pagedUsers = await ToPagedListAsync(
            query.OrderByDescending(user => user.CreatedAt),
            user => new AdminUserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName ?? string.Empty,
                LocationAddress = user.LocationAddress ?? string.Empty,
                CreatedAt = user.CreatedAt
            },
            search,
            page);

        var userIds = pagedUsers.Items.Select(user => user.Id).ToList();
        var roles = await _dbContext.UserRoles
            .Where(userRole => userIds.Contains(userRole.UserId))
            .Join(_dbContext.Roles, userRole => userRole.RoleId, role => role.Id, (userRole, role) => new { userRole.UserId, role.Name })
            .ToListAsync();

        foreach (var user in pagedUsers.Items)
        {
            user.Role = string.Join(", ", roles.Where(role => role.UserId == user.Id).Select(role => role.Name));
        }

        return pagedUsers;
    }

    private async Task LogAdminTableViewAsync(string message)
    {
        await _systemLogService.LogAsync(
            "AdminAction",
            message,
            userEmail: User.Identity?.Name,
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());
    }

    private static async Task<PagedListViewModel<TResult>> ToPagedListAsync<TSource, TResult>(
        IQueryable<TSource> query,
        Func<TSource, TResult> selector,
        string search,
        int page,
        int pageSize = 10)
    {
        var pageNumber = Math.Max(1, page);
        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        pageNumber = Math.Min(pageNumber, totalPages);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedListViewModel<TResult>
        {
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items.Select(selector).ToList()
        };
    }

    private static string DisplayUser(ApplicationUser? user)
    {
        if (user is null)
        {
            return "-";
        }

        return string.IsNullOrWhiteSpace(user.DisplayName)
            ? user.Email ?? "-"
            : $"{user.DisplayName} ({user.Email})";
    }
}
