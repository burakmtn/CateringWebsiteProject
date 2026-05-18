using System.Net;
using System.Net.Mail;
using System.Text;
using CateringWebsite.Data;
using CateringWebsite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CateringWebsite.Services;

public class SmtpOrderEmailService : IOrderEmailService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SmtpOrderEmailService> _logger;
    private readonly SmtpEmailOptions _options;
    private readonly ISystemLogService _systemLogService;

    public SmtpOrderEmailService(
        ApplicationDbContext dbContext,
        IOptions<SmtpEmailOptions> options,
        ILogger<SmtpOrderEmailService> logger,
        ISystemLogService systemLogService)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _logger = logger;
        _systemLogService = systemLogService;
    }

    public async Task SendOrderEmailsAsync(int orderId)
    {
        if (!IsConfigured())
        {
            _logger.LogInformation("Order email skipped because SMTP settings are not configured.");
            await _systemLogService.LogAsync(
                "EmailSkipped",
                $"Order email skipped for order #{orderId} because SMTP settings are not configured.",
                relatedEntityType: nameof(Order),
                relatedEntityId: orderId.ToString());
            return;
        }

        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .Include(order => order.Items)
                .ThenInclude(item => item.Caretaker)
            .Include(order => order.Items)
                .ThenInclude(item => item.SelectedOptions)
            .FirstOrDefaultAsync(order => order.Id == orderId);

        if (order is null)
        {
            _logger.LogWarning("Order email skipped because order {OrderId} was not found.", orderId);
            await _systemLogService.LogAsync(
                "EmailSkipped",
                $"Order email skipped because order #{orderId} was not found.",
                "Warning",
                relatedEntityType: nameof(Order),
                relatedEntityId: orderId.ToString());
            return;
        }

        await SendCustomerEmailAsync(order);
        await SendCaretakerEmailsAsync(order);
    }

    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_options.Host)
            && !string.IsNullOrWhiteSpace(_options.SenderEmail);
    }

    private async Task SendCustomerEmailAsync(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.User?.Email))
        {
            return;
        }

        var body = BuildOrderBody(
            order,
            "Your SofraLink order is complete.",
            order.Items);

        await SendEmailAsync(order.User.Email, "SofraLink order confirmation", body);
    }

    private async Task SendCaretakerEmailsAsync(Order order)
    {
        var caretakerGroups = order.Items
            .Where(item => !string.IsNullOrWhiteSpace(item.Caretaker?.Email))
            .GroupBy(item => item.CaretakerId);

        foreach (var group in caretakerGroups)
        {
            var caretaker = group.First().Caretaker!;
            var body = BuildOrderBody(
                order,
                "A new SofraLink order includes your menu items.",
                group);

            await SendEmailAsync(caretaker.Email!, $"New SofraLink order #{order.Id}", body);
        }
    }

    private string BuildOrderBody(Order order, string title, IEnumerable<OrderItem> items)
    {
        var builder = new StringBuilder();
        builder.AppendLine(title);
        builder.AppendLine();
        builder.AppendLine($"Order ID: {order.Id}");
        builder.AppendLine($"Customer: {DisplayUser(order.User)}");
        builder.AppendLine($"Delivery address: {ValueOrDash(order.DeliveryAddress)}");
        builder.AppendLine($"Purchase time: {order.CreatedAt:u}");
        builder.AppendLine($"Status: {order.Status}");
        builder.AppendLine();
        builder.AppendLine("Items:");

        foreach (var item in items)
        {
            builder.AppendLine($"- {item.MenuName} x{item.Quantity} | Unit: {Money(item.UnitPrice)} | Subtotal: {Money(item.Subtotal)}");
            builder.AppendLine($"  Caterer: {DisplayUser(item.Caretaker)}");

            foreach (var option in item.SelectedOptions)
            {
                builder.AppendLine($"  Option: {option.GroupName}: {option.Name} ({Money(option.PriceChange)})");
            }
        }

        builder.AppendLine();
        builder.AppendLine($"Total price: {Money(order.TotalAmount)}");
        return builder.ToString();
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_options.SenderEmail, _options.SenderName),
                Subject = subject,
                Body = body
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Timeout = 10000
            };

            if (!string.IsNullOrWhiteSpace(_options.UserName))
            {
                client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
            }

            await client.SendMailAsync(message);
            await _systemLogService.LogAsync("EmailSent", $"Order email sent to {toEmail}.");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to send order email to {Email}.", toEmail);
            await _systemLogService.LogAsync("EmailFailed", $"Order email failed for {toEmail}.", "Warning");
        }
    }

    private static string DisplayUser(ApplicationUser? user)
    {
        if (user is null)
        {
            return "-";
        }

        return string.IsNullOrWhiteSpace(user.DisplayName)
            ? ValueOrDash(user.Email)
            : user.DisplayName;
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
