using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
    public interface IBillService
    {
        Task<ServiceResponse<IEnumerable<BillDto>>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<ServiceResponse<BillDto>> GetByIdAsync(int id);
        Task<ServiceResponse<BillDto>> CreateAsync(BillDto billDto);
        Task<ServiceResponse<BillDto>> UpdateAsync(BillDto billDto);
        Task<ServiceResponse<bool>> DeleteAsync(int id);

    }
}
