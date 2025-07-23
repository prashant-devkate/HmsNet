using HmsNet.Helpers;
using HmsNet.Models.Domain;
using HmsNet.Models.DTO;
using HmsNet.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _userService;
        private readonly JwtHelper _jwtHelper;

        public AuthController(AuthService userService, JwtHelper jwtHelper)
        {
            _userService = userService;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var hashedPwd = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User
            {
                Username = dto.Username,
                Firstname = dto.Firstname,
                Lastname = dto.Lastname,
                Email = dto.Email,
                PasswordHash = hashedPwd,
                Role = dto.Role
            };

            var result = await _userService.Register(user);

            return result switch
            {
                "Success" => Ok("User registered successfully"),
                "Username already exists" => BadRequest("Username already exists"),
                "Email already exists" => BadRequest("Email already exists"),
                _ => BadRequest("Registration failed")
            };
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userService.Authenticate(dto.Username, dto.Password);
            if (user == null)
            {
                return Unauthorized(new ServiceResponse<string>
                {
                    Status = "Failed",
                    Message = "Invalid credentials"
                });
            }

            var token = _jwtHelper.GenerateToken(user.UserId, user.Username, user.Role);

            return Ok(new ServiceResponse<LoginResponseDto>
            {
                Status = "Success",
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


    }
}