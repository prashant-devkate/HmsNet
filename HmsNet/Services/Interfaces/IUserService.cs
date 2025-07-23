using HmsNet.Models.DTO;

namespace HmsNet.Services.Interfaces
{
	public interface IUserService
	{
		Task<IEnumerable<UserViewModel>> GetAllAsync();
		Task<ServiceResponse<string>> ChangePasswordAsync(ChangePasswordDTO dto);
		Task<ServiceResponse<string>> UpdateProfileAsync(UpdateProfileDTO dto);

	}
}