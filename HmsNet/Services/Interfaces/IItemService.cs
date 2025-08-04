using HmsNet.Models.Domain;
using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
    public interface IItemService
    {
        Task<ServiceResponse<IEnumerable<ItemDto>>> GetAllAsync();
        Task<ServiceResponse<IEnumerable<ItemDto>>> GetAllActiveAsync();
        Task<ServiceResponse<ItemDto>> GetByIdAsync(int id);
        Task<ServiceResponse<ItemDto>> CreateAsync(ItemDto item);
        Task<ServiceResponse<ItemDto>> UpdateAsync(ItemDto item);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
    }
}
