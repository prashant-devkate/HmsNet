using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResponse<OrderDto>> GetOrderByIdAsync(int orderId, bool includeDetails = false);
        Task<ServiceResponse<IEnumerable<OrderDto>>> GetAllOrdersAsync(int page = 1, int pageSize = 10, bool includeDetails = false);
        Task<ServiceResponse<OrderDto>> CreateOrderAsync(OrderDto orderDto);
        Task<ServiceResponse<OrderDto>> UpdateOrderAsync(OrderDto orderDto);
        Task<ServiceResponse<bool>> DeleteOrderAsync(int orderId);
        Task<ServiceResponse<IEnumerable<OrderDto>>> GetOrdersByRoomIdAsync(int roomId, int page = 1, int pageSize = 10, bool includeDetails = false);
        Task<ServiceResponse<decimal>> CalculateTotalAmountAsync(int orderId);
    }
}
