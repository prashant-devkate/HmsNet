using HmsNet.Data;
using HmsNet.Enums;
using HmsNet.Models.Domain;
using HmsNet.Models.DTO;
using HmsNet.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HmsNet.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private const bool UseSoftDelete = true; // Toggle for soft delete vs hard delete
        private const int MaxUsernameLength = 50;
        private const int MaxEmailLength = 255;

        public AuthService(AppDbContext context, ILogger<AuthService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool ValidateUser(User user, out string errorMessage)
        {
            if (user == null)
            {
                errorMessage = "User cannot be null";
                return false;
            }
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                errorMessage = "Username is required";
                return false;
            }
            if (user.Username.Length > MaxUsernameLength)
            {
                errorMessage = $"Username cannot exceed {MaxUsernameLength} characters";
                return false;
            }
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                errorMessage = "Email is required";
                return false;
            }
            if (user.Email.Length > MaxEmailLength)
            {
                errorMessage = $"Email cannot exceed {MaxEmailLength} characters";
                return false;
            }
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                errorMessage = "Password hash is required";
                return false;
            }
            if (string.IsNullOrWhiteSpace(user.Role))
            {
                errorMessage = "Role is required";
                return false;
            }
            errorMessage = null;
            return true;
        }

        public async Task<ServiceResponse<User>> RegisterAsync(User user)
        {
            var response = new ServiceResponse<User>();
            try
            {
                if (!ValidateUser(user, out var errorMessage))
                {
                    _logger.LogWarning("Validation failed for RegisterAsync: {Error}", errorMessage);
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                if (await _context.Users.AnyAsync(u => u.Username.Trim().ToLower() == user.Username.Trim().ToLower() && (!UseSoftDelete)))
                {
                    _logger.LogWarning("Registration failed: Username {Username} already exists", user.Username);
                    response.Status = ResponseStatus.Error;
                    response.Message = "Username already exists";
                    return response;
                }

                if (await _context.Users.AnyAsync(u => u.Email.Trim().ToLower() == user.Email.Trim().ToLower() && (!UseSoftDelete)))
                {
                    _logger.LogWarning("Registration failed: Email {Email} already exists", user.Email);
                    response.Status = ResponseStatus.Error;
                    response.Message = "Email already exists";
                    return response;
                }

                user.Username = user.Username.Trim();
                user.Email = user.Email.Trim();
                user.Firstname = user.Firstname?.Trim();
                user.Lastname = user.Lastname?.Trim();

                await using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = user;
                response.Status = ResponseStatus.Success;
                response.Message = "User registered successfully";
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error registering user {Username}: {Message}", user.Username, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error registering user: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error registering user {Username}: {Message}", user.Username, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Unexpected error during registration: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<User>> AuthenticateAsync(string username, string password)
        {
            var response = new ServiceResponse<User>();
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Authentication failed: Username or password is empty");
                    response.Status = ResponseStatus.Error;
                    response.Message = "Username and password are required";
                    return response;
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username.Trim().ToLower() == username.Trim().ToLower());

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("Authentication failed for username {Username}: Invalid credentials", username);
                    response.Status = ResponseStatus.Error;
                    response.Message = "Invalid credentials";
                    return response;
                }

                response.Data = user;
                response.Status = ResponseStatus.Success;
                response.Message = "Authentication successful";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user {Username}: {Message}", username, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error authenticating user: {ex.Message}";
                response.Data = null;
                return response;
            }
        }
    }
}