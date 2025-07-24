using HmsNet.Models.Domain;
using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
    public interface IItemService
    {
        Task<ServiceResponse<IEnumerable<Item>>> GetAllAsync();
        Task<ServiceResponse<Item>> GetByIdAsync(int id);
        Task<ServiceResponse<Item>> CreateAsync(Item item);
        Task<ServiceResponse<Item>> UpdateAsync(Item item);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
    }
}
