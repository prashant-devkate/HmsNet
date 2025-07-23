using HmsNet.Data;
using HmsNet.Models.Domain;
using HmsNet.Models.DTO;
using BCrypt.Net;
using HmsNet.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HmsNet.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserViewModel>> GetAllAsync()
        {
            try
            {
                return await _context.Users
                    .Select(s => new UserViewModel
                    {
                        UserId = s.UserId,
                        Username = s.Username,
                        Firstname = s.Firstname,
                        Lastname = s.Lastname,
                        Email = s.Email,
                        Role = s.Role,
                        CreatedAt = s.CreatedAt
                    }).ToListAsync();
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<UserViewModel>();
            }
        }

        public async Task<ServiceResponse<string>> ChangePasswordAsync(ChangePasswordDTO dto)
        {
            var response = new ServiceResponse<string>();

            try
            {
                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    response.Status = "Failed";
                    response.Message = "User not found.";
                    return response;
                }

                try
                {
                    bool isValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
                    if (!isValid)
                    {
                        response.Status = "Failed";
                        response.Message = "Current password is incorrect.";
                        return response;
                    }
                }
                catch (FormatException)
                {
                    response.Status = "Error";
                    response.Message = "Stored password is not hashed properly. Please contact admin or reset password.";
                    return response;
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await _context.SaveChangesAsync();

                response.Status = "Success";
                response.Message = "Password changed successfully.";
                response.Data = "Password updated.";
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }


        public async Task<ServiceResponse<string>> UpdateProfileAsync(UpdateProfileDTO dto)
        {
            var response = new ServiceResponse<string>();

            try
            {
                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    response.Status = "Failed";
                    response.Message = "User not found.";
                    return response;
                }

                user.Username = dto.Username;
                await _context.SaveChangesAsync();

                response.Status = "Success";
                response.Message = "Profile updated successfully.";
                response.Data = "Profile changes saved.";
                return response;
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Message = $"An error occurred: {ex.Message}";
                return response;
            }
        }


    }
}