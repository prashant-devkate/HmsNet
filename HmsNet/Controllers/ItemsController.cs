using HmsNet.Models.Domain;
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
        public async Task<ActionResult<ServiceResponse<IEnumerable<Item>>>> GetItems()
        {
            var response = await _service.GetAllAsync();
            return StatusCode(response.Status == "Success" ? 200 : 500, response);
        }

        // GET: api/Items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<Item>>> GetItem(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return StatusCode(response.Status == "Success" ? 200 : 404, response);
        }

        // POST: api/Items
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<Item>>> CreateItem(Item item)
        {
            var response = await _service.CreateAsync(item);
            if (response.Status == "Success")
            {
                return CreatedAtAction(nameof(GetItem), new { id = response.Data.ItemId }, response);
            }
            return StatusCode(response.Status == "Error" && response.Message.Contains("not found") ? 400 : 400, response);
        }

        // PUT: api/Items/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<Item>>> UpdateItem(int id, Item item)
        {
            if (id != item.ItemId)
            {
                var res = new ServiceResponse<Item>
                {
                    Status = "Error",
                    Message = "Item ID mismatch",
                    Data = null
                };
                return BadRequest(res);
            }

            var response = await _service.UpdateAsync(item);
            return StatusCode(response.Status == "Success" ? 200 : response.Message.Contains("not found") ? 404 : 400, response);
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteItem(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode(response.Status == "Success" ? 204 : response.Message.Contains("not found") ? 404 : 400, response);
        }
    }
}
