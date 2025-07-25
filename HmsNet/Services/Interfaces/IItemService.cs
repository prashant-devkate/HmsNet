using HmsNet.Models.Domain;
using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
    public interface IItemService
    {
        Task<ServiceResponse<IEnumerable<ItemDto>>> GetAllAsync(int page = 1, int pageSize = 10, bool includeOrderDetails = false);
        Task<ServiceResponse<ItemDto>> GetByIdAsync(int id, bool includeOrderDetails = false);
        Task<ServiceResponse<ItemDto>> CreateAsync(ItemDto item);
        Task<ServiceResponse<ItemDto>> UpdateAsync(ItemDto item);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
    }
}
