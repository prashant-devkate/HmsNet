using HmsNet.Enums;
using HmsNet.Helpers;
using HmsNet.Models.Domain;
using HmsNet.Models.DTO;
using HmsNet.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HmsNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly JwtHelper _jwtHelper;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, JwtHelper jwtHelper, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _jwtHelper = jwtHelper ?? throw new ArgumentNullException(nameof(jwtHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("register")]
        public async Task<ActionResult<ServiceResponse<User>>> Register(RegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for Register: {Errors}", ModelState);
                    return BadRequest(new ServiceResponse<User>
                    {
                        Status = ResponseStatus.Error,
                        Message = "Invalid registration data",
                        Data = null
                    });
                }

                var user = new User
                {
                    Username = dto.Username.Trim(),
                    Firstname = dto.Firstname.Trim(),
                    Lastname = dto.Lastname.Trim(),
                    Email = dto.Email.Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = dto.Role
                };

                var response = await _authService.RegisterAsync(user);
                return response.Status == ResponseStatus.Success
                    ? Ok(response)
                    : BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username {Username}: {Message}", dto.Username, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ServiceResponse<User>
                {
                    Status = ResponseStatus.Error,
                    Message = "An error occurred during registration",
                    Data = null
                });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ServiceResponse<LoginResponseDto>>> Login(LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for Login: {Errors}", ModelState);
                    return BadRequest(new ServiceResponse<LoginResponseDto>
                    {
                        Status = ResponseStatus.Error,
                        Message = "Invalid login data",
                        Data = null
                    });
                }

                var response = await _authService.AuthenticateAsync(dto.Username, dto.Password);
                if (response.Status == ResponseStatus.Error)
                {
                    _logger.LogWarning("Authentication failed for username {Username}: {Message}", dto.Username, response.Message);
                    return Unauthorized(response);
                }

                var user = response.Data;
                var token = _jwtHelper.GenerateToken(user.UserId, user.Username, user.Role);

                return Ok(new ServiceResponse<LoginResponseDto>
                {
                    Status = ResponseStatus.Success,
                    Message = "Login successful",
                    Data = new LoginResponseDto
                    {
                        Token = token,
                        UserId = user.UserId,
                        Username = user.Username,
                        Firstname = user.Firstname,
                        Lastname = user.Lastname,
                        Email = user.Email,
                        Role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username {Username}: {Message}", dto.Username, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ServiceResponse<LoginResponseDto>
                {
                    Status = ResponseStatus.Error,
                    Message = "An error occurred during login",
                    Data = null
                });
            }
        }
    }
}