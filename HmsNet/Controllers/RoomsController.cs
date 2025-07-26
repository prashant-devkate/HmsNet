using HmsNet.Enums;
using HmsNet.Models.DTO;
using HmsNet.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HmsNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _service;

        public RoomsController(IRoomService service)
        {
            _service = service;
        }

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<IEnumerable<RoomDto>>>> GetRooms(int page = 1, int pageSize = 10, bool includeOrders = false)
        {
            var response = await _service.GetAllAsync(page, pageSize, includeOrders);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        // GET: api/Rooms/Available
        [HttpGet("Available")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<RoomDto>>>> GetActiveRooms(int page = 1, int pageSize = 10, bool includeOrders = false)
        {
            var response = await _service.GetAllActiveAsync(page, pageSize, includeOrders);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<RoomDto>>> GetRoom(int id, bool includeOrders = false)
        {
            var response = await _service.GetByIdAsync(id, includeOrders);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : NotFound(response);
        }

        // POST: api/Rooms
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<RoomDto>>> CreateRoom(RoomDto roomDto)
        {
            var response = await _service.CreateAsync(roomDto);
            return response.Status == ResponseStatus.Success
                ? CreatedAtAction(nameof(GetRoom), new { id = response.Data.RoomId }, response)
                : BadRequest(response);
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<RoomDto>>> UpdateRoom(int id, RoomDto roomDto)
        {
            if (id != roomDto.RoomId)
            {
                var res = new ServiceResponse<RoomDto>
                {
                    Status = ResponseStatus.Error,
                    Message = "Room ID mismatch",
                    Data = null
                };
                return BadRequest(res);
            }

            var response = await _service.UpdateAsync(roomDto);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : response.Message.Contains("not found")
                    ? NotFound(response)
                    : BadRequest(response);
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteRoom(int id)
        {
            var response = await _service.DeleteAsync(id);
            return response.Status == ResponseStatus.Success
                ? NoContent()
                : response.Message.Contains("not found")
                    ? NotFound(response)
                    : BadRequest(response);
        }
    }
}