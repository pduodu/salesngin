namespace salesngin.Services.Interfaces;

public interface IOrderService
{
    //PAGE GET ACTIONS
    Task<OrderViewModel> GetCreateOrderPageAsync(int orderId, string itemType = null, CancellationToken ct = default);
    Task<OrderViewModel> GetOrderDetailsPageAsync(int orderId, CancellationToken ct = default);

    //POST ACTIONS
    Task<OperationResult> CreateOrderAsync(Order order, CancellationToken ct = default);
    Task<OperationResult> UpdateOrderAsync(Order order, CancellationToken ct = default);

    Task<OperationResult<int>> PostCreateOrderPageAsync(OrderViewModel input, ApplicationUser user, CancellationToken ct = default);
    Task<OperationResult<int>> PostOrderCheckoutPageAsync(OrderViewModel input, ApplicationUser user, CancellationToken ct = default);
    Task<OperationResult> PostOrderStatusUpdatePageAsync(OrderViewModel input, CancellationToken ct = default);
    Task<OperationResult> DeleteOrderAsync(int orderId, CancellationToken ct = default);

    //GET ACTIONS
    Task<Order> GetOrderByIdAsync(int orderId, CancellationToken ct = default);
    Task<List<Order>> GetUserOrdersAsync(int userId, CancellationToken ct = default);
    Task<List<Order>> GetOrdersAsync(CancellationToken ct = default);
    OrderTotals CalculateOrderTotals(
        IEnumerable<OrderItem> orderItems,
        decimal discountAmount,
        decimal totalCharges,
        decimal vatPercent,
        decimal taxAmount);
}