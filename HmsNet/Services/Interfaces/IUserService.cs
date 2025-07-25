using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
	public interface IUserService
	{
        Task<ServiceResponse<IEnumerable<UserViewModel>>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<ServiceResponse<string>> ChangePasswordAsync(ChangePasswordDTO dto);
        Task<ServiceResponse<string>> UpdateProfileAsync(UpdateProfileDTO dto);

    }
}