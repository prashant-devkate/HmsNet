using HmsNet.Models.Domain;
using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
    public interface IRoomService
    {
        Task<ServiceResponse<IEnumerable<RoomDto>>> GetAllAsync(int page = 1, int pageSize = 10, bool includeOrders = false);
        Task<ServiceResponse<RoomDto>> GetByIdAsync(int id, bool includeOrders = false);
        Task<ServiceResponse<RoomDto>> CreateAsync(RoomDto room);
        Task<ServiceResponse<RoomDto>> UpdateAsync(RoomDto room);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
    }
}
