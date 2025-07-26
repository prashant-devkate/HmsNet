using HmsNet.Enums;
using HmsNet.Models.DTO;
using HmsNet.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HmsNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _service;

        public ItemsController(IItemService service)
        {
            _service = service;
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<IEnumerable<ItemDto>>>> GetItems(int page = 1, int pageSize = 10, bool includeOrderDetails = false)
        {
            var response = await _service.GetAllAsync(page, pageSize, includeOrderDetails);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        // GET: api/Items/Active
        [HttpGet("Active")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<ItemDto>>>> GetActiveItems(int page = 1, int pageSize = 10, bool includeOrderDetails = false)
        {
            var response = await _service.GetAllActiveAsync(page, pageSize, includeOrderDetails);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        // GET: api/Items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<ItemDto>>> GetItem(int id, bool includeOrderDetails = false)
        {
            var response = await _service.GetByIdAsync(id, includeOrderDetails);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : NotFound(response);
        }

        // POST: api/Items
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<ItemDto>>> CreateItem(ItemDto itemDto)
        {
            var response = await _service.CreateAsync(itemDto);
            return response.Status == ResponseStatus.Success
                ? CreatedAtAction(nameof(GetItem), new { id = response.Data.ItemId }, response)
                : BadRequest(response);
        }

        // PUT: api/Items/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<ItemDto>>> UpdateItem(int id, ItemDto itemDto)
        {
            if (id != itemDto.ItemId)
            {
                var res = new ServiceResponse<ItemDto>
                {
                    Status = ResponseStatus.Error,
                    Message = "Item ID mismatch",
                    Data = null
                };
                return BadRequest(res);
            }

            var response = await _service.UpdateAsync(itemDto);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : response.Message.Contains("not found")
                    ? NotFound(response)
                    : BadRequest(response);
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteItem(int id)
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