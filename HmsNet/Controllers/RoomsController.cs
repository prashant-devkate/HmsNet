using HmsNet.Models.Domain;
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
        public async Task<ActionResult<ServiceResponse<IEnumerable<Room>>>> GetRooms()
        {
            var response = await _service.GetAllAsync();
            return StatusCode(response.Status == "Success" ? 200 : 500, response);
        }

        // GET: api/Rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<Room>>> GetRoom(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode(response.Status == "Success" ? 200 : 404, response);
        }

        // POST: api/Rooms
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<Room>>> CreateRoom(Room room)
        {
            var response = await _service.CreateAsync(room);
            if (response.Status == "Success")
            {
                return CreatedAtAction(nameof(GetRoom), new { id = response.Data.RoomId }, response);
            }
            return StatusCode(response.Status == "Error" && response.Message.Contains("not found") ? 400 : 400, response);
        }

        // PUT: api/Rooms/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<Room>>> UpdateRoom(int id, Room room)
        {
            if (id != room.RoomId)
            {
                var res = new ServiceResponse<Room>
                {
                    Status = "Error",
                    Message = "Room ID mismatch",
                    Data = null
                };
                return BadRequest(res);
            }

            var response = await _service.UpdateAsync(room);
            return StatusCode(response.Status == "Success" ? 200 : response.Message.Contains("not found") ? 404 : 400, response);
        }

        // DELETE: api/Rooms/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteRoom(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode(response.Status == "Success" ? 204 : response.Message.Contains("not found") ? 404 : 400, response);
        }
    }
}
