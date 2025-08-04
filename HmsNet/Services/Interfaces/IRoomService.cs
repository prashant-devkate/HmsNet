using HmsNet.Models.Domain;
using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
    public interface IRoomService
    {
        Task<ServiceResponse<IEnumerable<RoomDto>>> GetAllAsync();
        Task<ServiceResponse<IEnumerable<RoomDto>>> GetAllActiveAsync();
        Task<ServiceResponse<RoomDto>> GetByIdAsync(int id);
        Task<ServiceResponse<RoomDto>> CreateAsync(RoomDto room);
        Task<ServiceResponse<RoomDto>> UpdateAsync(RoomDto room);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
        Task<ServiceResponse<RoomDto>> UpdateStatusAsync(int id, string status);
        Task<ServiceResponse<RoomDto>> UpdateOrderIdAsync(int id, int orderId);
    }
}
