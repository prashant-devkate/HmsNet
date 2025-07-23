using HmsNet.Models.Domain;
using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
    public interface IRoomService
    {
        Task<ServiceResponse<IEnumerable<Room>>> GetAllAsync();
        Task<ServiceResponse<Room>> GetByIdAsync(int id);
        Task<ServiceResponse<Room>> CreateAsync(Room room);
        Task<ServiceResponse<Room>> UpdateAsync(Room room);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
    }
}
