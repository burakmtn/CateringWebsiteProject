namespace CateringWebsite.Services;

public interface IOrderEmailService
{
    Task SendOrderEmailsAsync(int orderId);
}
