using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
    public interface IOrderDetailService
    {
        Task<ServiceResponse<OrderDetailDto>> GetOrderDetailsByIdAsync(int orderDetailId, bool includeDetails = false);
        Task<ServiceResponse<IEnumerable<OrderDetailDto>>> GetAllOrderDetailsAsync(int page = 1, int pageSize = 10, bool includeDetails = false);
        Task<ServiceResponse<OrderDetailDto>> CreateOrderDetailsAsync(OrderDetailDto orderDto);
        Task<ServiceResponse<OrderDetailDto>> UpdateOrderDetailsAsync(OrderDetailDto orderDto);
        Task<ServiceResponse<bool>> DeleteOrderDetailsAsync(int orderDetailId);
        Task<ServiceResponse<List<OrderDetailDto>>> GetOrderDetailsByOrderIdAsync(int orderId, bool includeDetails = false);
    }
}
