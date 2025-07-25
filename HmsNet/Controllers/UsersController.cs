using HmsNet.Enums;
using HmsNet.Models.DTO;
using HmsNet.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HmsNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService service, ILogger<UsersController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResponse<IEnumerable<UserViewModel>>>> GetAll(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _service.GetAllAsync(page, pageSize);
                return response.Status == ResponseStatus.Success
                    ? Ok(response)
                    : StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ServiceResponse<IEnumerable<UserViewModel>>
                {
                    Status = ResponseStatus.Error,
                    Message = $"Error retrieving users: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpPost("ChangePassword")]
        public async Task<ActionResult<ServiceResponse<string>>> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ChangePassword: {Errors}", ModelState);
                    return BadRequest(new ServiceResponse<string>
                    {
                        Status = ResponseStatus.Error,
                        Message = "Invalid change password data",
                        Data = null
                    });
                }

                var response = await _service.ChangePasswordAsync(dto);
                return response.Status == ResponseStatus.Success
                    ? Ok(response)
                    : BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user ID {UserId}: {Message}", dto.UserId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ServiceResponse<string>
                {
                    Status = ResponseStatus.Error,
                    Message = $"Error changing password: {ex.Message}",
                    Data = null
                });
            }
        }

        [HttpPost("UpdateProfile")]
        public async Task<ActionResult<ServiceResponse<string>>> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateProfile: {Errors}", ModelState);
                    return BadRequest(new ServiceResponse<string>
                    {
                        Status = ResponseStatus.Error,
                        Message = "Invalid update profile data",
                        Data = null
                    });
                }

                var response = await _service.UpdateProfileAsync(dto);
                return response.Status == ResponseStatus.Success
                    ? Ok(response)
                    : BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user ID {UserId}: {Message}", dto.UserId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ServiceResponse<string>
                {
                    Status = ResponseStatus.Error,
                    Message = $"Error updating profile: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}