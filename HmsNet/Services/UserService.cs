using BCrypt.Net;
using HmsNet.Data;
using HmsNet.Enums;
using HmsNet.Models.Domain;
using HmsNet.Models.DTO;
using HmsNet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HmsNet.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;
        private const int MaxUsernameLength = 50;
        private const bool UseSoftDelete = true; // Added for consistency with other services

        public UserService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResponse<IEnumerable<UserViewModel>>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var response = new ServiceResponse<IEnumerable<UserViewModel>>();
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Invalid page or pageSize";
                    return response;
                }

                var query = _context.Users.AsQueryable();
                if (UseSoftDelete)
                {
                    query = query.Where(u => u.Status == "Active");
                }

                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new UserViewModel
                    {
                        UserId = s.UserId,
                        Username = s.Username,
                        Firstname = s.Firstname,
                        Lastname = s.Lastname,
                        Email = s.Email,
                        Role = s.Role,
                        Status = s.Status,
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();

                response.Data = users;
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving users: {ex.Message}";
                response.Data = Enumerable.Empty<UserViewModel>();
                return response;
            }
        }

        public async Task<ServiceResponse<string>> ChangePasswordAsync(ChangePasswordDTO dto)
        {
            var response = new ServiceResponse<string>();

            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Current and new passwords are required.";
                    return response;
                }

                if (dto.NewPassword.Length < 6)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "New password must be at least 6 characters long.";
                    return response;
                }

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "User not found.";
                    return response;
                }

                try
                {
                    bool isValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
                    if (!isValid)
                    {
                        response.Status = ResponseStatus.Error;
                        response.Message = "Current password is incorrect.";
                        return response;
                    }
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Invalid password hash format for user ID {UserId}: {Message}", dto.UserId, ex.Message);
                    response.Status = ResponseStatus.Error;
                    response.Message = "Stored password is not hashed properly. Please contact admin or reset password.";
                    return response;
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Status = ResponseStatus.Success;
                response.Message = "Password changed successfully.";
                response.Data = "Password updated.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error changing password for user ID {UserId}: {Message}", dto.UserId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error changing password: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error changing password for user ID {UserId}: {Message}", dto.UserId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<string>> UpdateProfileAsync(UpdateProfileDTO dto)
        {
            var response = new ServiceResponse<string>();

            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Username))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Username is required.";
                    return response;
                }

                if (dto.Username.Length > MaxUsernameLength)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Username cannot exceed {MaxUsernameLength} characters.";
                    return response;
                }

                if (await _context.Users.AnyAsync(u => u.Username.Trim().ToLower() == dto.Username.Trim().ToLower() && u.UserId != dto.UserId))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Username already exists.";
                    return response;
                }

                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "User not found.";
                    return response;
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();
                user.Username = dto.Username.Trim();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Status = ResponseStatus.Success;
                response.Message = "Profile updated successfully.";
                response.Data = "Profile changes saved.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating profile for user ID {UserId}: {Message}", dto.UserId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error updating profile: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating profile for user ID {UserId}: {Message}", dto.UserId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"An unexpected error occurred: {ex.Message}";
            }

            return response;
        }
    }
}