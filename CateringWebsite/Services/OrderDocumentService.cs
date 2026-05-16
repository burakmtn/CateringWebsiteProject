using CateringWebsite.Data;
using CateringWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace CateringWebsite.Services;

public class OrderDocumentService : IOrderDocumentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<OrderDocumentService> _logger;

    public OrderDocumentService(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment,
        ILogger<OrderDocumentService> logger)
    {
        _dbContext = dbContext;
        _environment = environment;
        _logger = logger;
    }

    public async Task GenerateOrderDocumentsAsync(int orderId)
    {
        var order = await LoadOrderAsync(orderId);
        if (order is null)
        {
            _logger.LogWarning("Order documents were not generated because order {OrderId} was not found.", orderId);
            return;
        }

        Directory.CreateDirectory(GetOrderDirectory(order.Id));
        SimplePdfWriter.Write(GetDocumentPath(order.Id, OrderDocumentKind.Receipt), BuildReceiptLines(order));
        SimplePdfWriter.Write(GetDocumentPath(order.Id, OrderDocumentKind.Agreement), BuildAgreementLines(order));
    }

    public async Task<OrderDocumentFile?> GetOrderDocumentAsync(int orderId, OrderDocumentKind kind)
    {
        var path = GetDocumentPath(orderId, kind);
        if (!File.Exists(path))
        {
            await GenerateOrderDocumentsAsync(orderId);
        }

        return File.Exists(path)
            ? new OrderDocumentFile(path, Path.GetFileName(path))
            : null;
    }

    private async Task<Order?> LoadOrderAsync(int orderId)
    {
        return await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .Include(order => order.Items)
                .ThenInclude(item => item.Caretaker)
            .Include(order => order.Items)
                .ThenInclude(item => item.SelectedOptions)
            .FirstOrDefaultAsync(order => order.Id == orderId);
    }

    private IReadOnlyList<string> BuildReceiptLines(Order order)
    {
        var lines = new List<string>
        {
            "SofraLink Receipt Form",
            string.Empty,
            $"Order ID: {order.Id}",
            $"Order status: {order.Status}",
            $"Purchase time: {order.CreatedAt:u}",
            $"Generated at: {DateTime.UtcNow:u}",
            string.Empty,
            "Customer",
            $"Name: {DisplayUser(order.User)}",
            $"Delivery address: {ValueOrDash(order.DeliveryAddress)}",
            string.Empty,
            "Ordered Items"
        };

        AddItemLines(lines, order.Items);
        lines.Add(string.Empty);
        lines.Add($"Total price: {Money(order.TotalAmount)}");
        return lines;
    }

    private IReadOnlyList<string> BuildAgreementLines(Order order)
    {
        var caterers = order.Items
            .Select(item => DisplayUser(item.Caretaker))
            .Distinct()
            .ToList();

        var lines = new List<string>
        {
            "SofraLink Customer-Caterer Agreement",
            string.Empty,
            $"Agreement for order #{order.Id}",
            $"Generated at: {DateTime.UtcNow:u}",
            string.Empty,
            "Parties",
            $"Customer: {DisplayUser(order.User)}",
            $"Caterer(s): {string.Join(", ", caterers)}",
            string.Empty,
            "Order Summary",
            $"Purchase time: {order.CreatedAt:u}",
            $"Order status: {order.Status}",
            $"Delivery address: {ValueOrDash(order.DeliveryAddress)}",
            $"Total order amount: {Money(order.TotalAmount)}",
            string.Empty,
            "Agreement Details",
            "This document was generated dynamically for the completed SofraLink order.",
            "The customer and caterer agree that the listed order data is the shared purchase record.",
            "Payment is simulated inside the application and no real card data is stored.",
            string.Empty,
            "Included Items"
        };

        AddItemLines(lines, order.Items);
        return lines;
    }

    private static void AddItemLines(List<string> lines, IEnumerable<OrderItem> items)
    {
        foreach (var item in items)
        {
            lines.Add($"- {item.MenuName}");
            lines.Add($"  Caterer: {DisplayUser(item.Caretaker)}");
            lines.Add($"  Quantity: {item.Quantity}");
            lines.Add($"  Unit price: {Money(item.UnitPrice)}");
            lines.Add($"  Subtotal: {Money(item.Subtotal)}");

            foreach (var option in item.SelectedOptions)
            {
                lines.Add($"  Option: {option.GroupName}: {option.Name} ({Money(option.PriceChange)})");
            }
        }
    }

    private string GetDocumentPath(int orderId, OrderDocumentKind kind)
    {
        var suffix = kind == OrderDocumentKind.Receipt ? "receipt" : "agreement";
        return Path.Combine(GetOrderDirectory(orderId), $"order-{orderId}-{suffix}.pdf");
    }

    private string GetOrderDirectory(int orderId)
    {
        return Path.Combine(_environment.ContentRootPath, "GeneratedDocuments", "orders", $"order-{orderId}");
    }

    private static string DisplayUser(ApplicationUser? user)
    {
        if (user is null)
        {
            return "-";
        }

        return string.IsNullOrWhiteSpace(user.DisplayName)
            ? ValueOrDash(user.Email)
            : $"{user.DisplayName} ({ValueOrDash(user.Email)})";
    }

    private static string ValueOrDash(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string Money(decimal amount)
    {
        return amount.ToString("0.00");
    }
}
