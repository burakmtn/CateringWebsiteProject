using System.Text.Json;
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
public class CartController : Controller
{
    private const string CartSessionKey = "SofraLink.Cart";

    private readonly ApplicationDbContext _dbContext;
    private readonly IOrderDocumentService _orderDocumentService;
    private readonly IOrderEmailService _orderEmailService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(
        ApplicationDbContext dbContext,
        IOrderDocumentService orderDocumentService,
        IOrderEmailService orderEmailService,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _orderDocumentService = orderDocumentService;
        _orderEmailService = orderEmailService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        return View(await BuildCartViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddToCartViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(UserController.Details), "User", new { id = model.MenuItemId });
        }

        var menuItem = await _dbContext.MenuItems
            .Include(item => item.CustomizationOptions)
            .FirstOrDefaultAsync(item => item.Id == model.MenuItemId);

        if (menuItem is null)
        {
            return NotFound();
        }

        var validOptionIds = menuItem.CustomizationOptions.Select(option => option.Id).ToHashSet();
        var selectedOptionIds = model.SelectedOptionIds
            .Where(validOptionIds.Contains)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var cart = GetCart();
        var existingItem = cart.FirstOrDefault(item =>
            item.MenuItemId == model.MenuItemId &&
            item.SelectedOptionIds.OrderBy(id => id).SequenceEqual(selectedOptionIds));

        if (existingItem is null)
        {
            cart.Add(new CartSessionItem
            {
                MenuItemId = model.MenuItemId,
                Quantity = model.Quantity,
                SelectedOptionIds = selectedOptionIds
            });
        }
        else
        {
            existingItem.Quantity += model.Quantity;
        }

        SaveCart(cart);
        TempData["StatusMessage"] = "Item added to cart.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(string cartItemId, int quantity)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(cartItem => cartItem.Id == cartItemId);
        if (item is not null)
        {
            item.Quantity = Math.Clamp(quantity, 1, 99);
            SaveCart(cart);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(string cartItemId)
    {
        var cart = GetCart();
        cart.RemoveAll(item => item.Id == cartItemId);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Payment()
    {
        var cartViewModel = await BuildCartViewModelAsync();
        if (cartViewModel.Items.Count == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new PaymentViewModel { Cart = cartViewModel });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Payment(PaymentViewModel model)
    {
        var cartViewModel = await BuildCartViewModelAsync();
        model.Cart = cartViewModel;

        if (cartViewModel.Items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var order = await CreateOrderAsync(user, GetCart(), cartViewModel.TotalAmount);
        await _orderDocumentService.GenerateOrderDocumentsAsync(order.Id);
        await _orderEmailService.SendOrderEmailsAsync(order.Id);
        ClearCart();

        return RedirectToAction(nameof(Confirmation), new { id = order.Id });
    }

    public async Task<IActionResult> Confirmation(int id)
    {
        var userId = _userManager.GetUserId(User);
        var order = await _dbContext.Orders.FirstOrDefaultAsync(order => order.Id == id && order.UserId == userId);
        if (order is null)
        {
            return NotFound();
        }

        return View(new OrderConfirmationViewModel
        {
            OrderId = order.Id,
            Status = order.Status,
            TotalAmount = order.TotalAmount
        });
    }

    private async Task<Order> CreateOrderAsync(ApplicationUser user, IReadOnlyList<CartSessionItem> cart, decimal totalAmount)
    {
        var menuItemIds = cart.Select(item => item.MenuItemId).Distinct().ToList();
        var menuItems = await _dbContext.MenuItems
            .Include(item => item.Caretaker)
            .Include(item => item.CustomizationOptions)
            .Where(item => menuItemIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id);

        var order = new Order
        {
            UserId = user.Id,
            DeliveryAddress = user.LocationAddress ?? string.Empty,
            Status = "Completed",
            TotalAmount = totalAmount
        };

        foreach (var cartItem in cart)
        {
            if (!menuItems.TryGetValue(cartItem.MenuItemId, out var menuItem))
            {
                continue;
            }

            var selectedOptions = menuItem.CustomizationOptions
                .Where(option => cartItem.SelectedOptionIds.Contains(option.Id))
                .ToList();
            var unitPrice = menuItem.Price + selectedOptions.Sum(option => option.PriceChange);

            var orderItem = new OrderItem
            {
                MenuItemId = menuItem.Id,
                CaretakerId = menuItem.CaretakerId,
                MenuName = menuItem.Name,
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                Subtotal = unitPrice * cartItem.Quantity
            };

            foreach (var option in selectedOptions)
            {
                orderItem.SelectedOptions.Add(new OrderItemOption
                {
                    MenuCustomizationOptionId = option.Id,
                    GroupName = option.GroupName,
                    Name = option.Name,
                    PriceChange = option.PriceChange
                });
            }

            order.Items.Add(orderItem);
        }

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        return order;
    }

    private async Task<CartViewModel> BuildCartViewModelAsync()
    {
        var cart = GetCart();
        if (cart.Count == 0)
        {
            return new CartViewModel();
        }

        var menuItemIds = cart.Select(item => item.MenuItemId).Distinct().ToList();
        var menuItems = await _dbContext.MenuItems
            .Include(item => item.Caretaker)
            .Include(item => item.CustomizationOptions)
            .Where(item => menuItemIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id);

        var items = new List<CartItemViewModel>();
        foreach (var cartItem in cart)
        {
            if (!menuItems.TryGetValue(cartItem.MenuItemId, out var menuItem))
            {
                continue;
            }

            var selectedOptions = menuItem.CustomizationOptions
                .Where(option => cartItem.SelectedOptionIds.Contains(option.Id))
                .ToList();
            var optionsTotal = selectedOptions.Sum(option => option.PriceChange);
            var unitPrice = menuItem.Price + optionsTotal;

            items.Add(new CartItemViewModel
            {
                CartItemId = cartItem.Id,
                MenuItemId = menuItem.Id,
                MenuName = menuItem.Name,
                CaretakerName = menuItem.Caretaker?.DisplayName ?? menuItem.Caretaker?.Email ?? "Caretaker",
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                OptionsTotal = optionsTotal,
                Subtotal = unitPrice * cartItem.Quantity,
                SelectedOptions = selectedOptions
                    .Select(option => option.PriceChange == 0
                        ? $"{option.GroupName}: {option.Name}"
                        : $"{option.GroupName}: {option.Name} ({option.PriceChange:+0.##;-0.##})")
                    .ToList()
            });
        }

        return new CartViewModel { Items = items };
    }

    private List<CartSessionItem> GetCart()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<CartSessionItem>>(json) ?? [];
    }

    private void SaveCart(List<CartSessionItem> cart)
    {
        HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
    }

    private void ClearCart()
    {
        HttpContext.Session.Remove(CartSessionKey);
    }
}
