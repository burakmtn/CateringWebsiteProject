namespace CateringWebsite.Services;

public interface IOrderDocumentService
{
    Task GenerateOrderDocumentsAsync(int orderId);

    Task<OrderDocumentFile?> GetOrderDocumentAsync(int orderId, OrderDocumentKind kind);
}
