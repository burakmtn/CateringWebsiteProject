using CateringWebsite.Data;
using CateringWebsite.Models;
using CateringWebsite.Models.ViewModels;
using CateringWebsite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringWebsite.Controllers;

[Authorize(Roles = AppRoles.User)]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOrderDocumentService _orderDocumentService;
    private readonly ISystemLogService _systemLogService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(
        ApplicationDbContext dbContext,
        IOrderDocumentService orderDocumentService,
        ISystemLogService systemLogService,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _orderDocumentService = orderDocumentService;
        _systemLogService = systemLogService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var orderItems = await _dbContext.OrderItems
            .Include(item => item.Order)
            .Include(item => item.Caretaker)
            .Include(item => item.Review)
            .Where(item => item.Order != null && item.Order.UserId == userId)
            .OrderByDescending(item => item.Order!.CreatedAt)
            .ToListAsync();

        var viewModel = new OrderHistoryViewModel
        {
            Items = orderItems.Select(item => new OrderHistoryItemViewModel
            {
                OrderId = item.OrderId,
                OrderItemId = item.Id,
                MenuName = item.MenuName,
                CaretakerName = item.Caretaker?.DisplayName ?? item.Caretaker?.Email ?? "Caretaker",
                Quantity = item.Quantity,
                Subtotal = item.Subtotal,
                Status = item.Order?.Status ?? string.Empty,
                CreatedAt = item.Order?.CreatedAt ?? DateTime.UtcNow,
                CanReview = item.Order?.Status == "Completed" && item.Review is null,
                HasReview = item.Review is not null
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Document(int orderId, string kind)
    {
        var userId = _userManager.GetUserId(User);
        var ownsOrder = await _dbContext.Orders
            .AnyAsync(order => order.Id == orderId && order.UserId == userId);

        if (!ownsOrder)
        {
            return NotFound();
        }

        if (!Enum.TryParse<OrderDocumentKind>(kind, true, out var documentKind))
        {
            return NotFound();
        }

        var document = await _orderDocumentService.GetOrderDocumentAsync(orderId, documentKind);
        if (document is null)
        {
            return NotFound();
        }

        return PhysicalFile(document.Path, "application/pdf", document.FileName);
    }

    [HttpGet]
    public async Task<IActionResult> Review(int orderItemId)
    {
        var orderItem = await FindReviewableOrderItemAsync(orderItemId);
        if (orderItem is null)
        {
            return NotFound();
        }

        if (orderItem.Review is not null || orderItem.Order?.Status != "Completed")
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new ReviewFormViewModel
        {
            OrderItemId = orderItem.Id,
            MenuName = orderItem.MenuName,
            CaretakerName = orderItem.Caretaker?.DisplayName ?? orderItem.Caretaker?.Email ?? "Caretaker"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(ReviewFormViewModel model)
    {
        var orderItem = await FindReviewableOrderItemAsync(model.OrderItemId);
        if (orderItem is null)
        {
            return NotFound();
        }

        model.MenuName = orderItem.MenuName;
        model.CaretakerName = orderItem.Caretaker?.DisplayName ?? orderItem.Caretaker?.Email ?? "Caretaker";

        if (orderItem.Review is not null)
        {
            ModelState.AddModelError(string.Empty, "This order item has already been reviewed.");
        }

        if (orderItem.Order?.Status != "Completed")
        {
            ModelState.AddModelError(string.Empty, "Only completed orders can be reviewed.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var review = new OrderItemReview
        {
            OrderItemId = orderItem.Id,
            UserId = userId,
            MenuItemId = orderItem.MenuItemId,
            CaretakerId = orderItem.CaretakerId,
            MenuRating = model.MenuRating,
            CaretakerRating = model.CaretakerRating,
            Comment = model.Comment.Trim()
        };

        _dbContext.OrderItemReviews.Add(review);
        await _dbContext.SaveChangesAsync();
        await _systemLogService.LogAsync(
            "RatingSubmitted",
            $"Rating submitted for order item #{orderItem.Id}.",
            user: orderItem.Order?.User,
            relatedEntityType: nameof(OrderItem),
            relatedEntityId: orderItem.Id.ToString(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["StatusMessage"] = "Review submitted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<OrderItem?> FindReviewableOrderItemAsync(int orderItemId)
    {
        var userId = _userManager.GetUserId(User);
        return await _dbContext.OrderItems
            .Include(item => item.Order)
                .ThenInclude(order => order!.User)
            .Include(item => item.Caretaker)
            .Include(item => item.Review)
            .FirstOrDefaultAsync(item =>
                item.Id == orderItemId &&
                item.Order != null &&
                item.Order.UserId == userId);
    }
}
