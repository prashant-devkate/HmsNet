using HmsNet.Enums;
using HmsNet.Models.DTO;
using HmsNet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HmsNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        // GET: api/orders
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeDetails = false)
        {
            var response = await _orderService.GetAllOrdersAsync(page, pageSize, includeDetails);
            return response.Status == ResponseStatus.Success ? Ok(response.Data) : StatusCode(500, new { message = response.Message });
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromQuery] bool includeDetails = false)
        {
            var response = await _orderService.GetOrderByIdAsync(id, includeDetails);
            return response.Status == ResponseStatus.Success ? Ok(response.Data) : NotFound(new { message = response.Message });
        }

        // GET: api/orders/room/5
        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetOrdersByRoomId(int roomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeDetails = false)
        {
            var response = await _orderService.GetOrdersByRoomIdAsync(roomId, page, pageSize, includeDetails);
            return response.Status == ResponseStatus.Success ? Ok(response.Data) : StatusCode(500, new { message = response.Message });
        }

        // GET: api/orders/5/total
        [HttpGet("{id}/total")]
        public async Task<IActionResult> GetTotalAmount(int id)
        {
            var response = await _orderService.CalculateTotalAmountAsync(id);
            return response.Status == ResponseStatus.Success ? Ok(response.Data) : NotFound(new { message = response.Message });
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderDto orderDto)
        {
            if (orderDto == null)
            {
                return BadRequest(new { message = "Order data is required" });
            }

            var response = await _orderService.CreateOrderAsync(orderDto);
            return response.Status == ResponseStatus.Success ? CreatedAtAction(nameof(GetById), new { id = response.Data.OrderId }, response.Data) : BadRequest(new { message = response.Message });
        }

        // PUT: api/orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrderDto orderDto)
        {
            if (orderDto == null || orderDto.OrderId != id)
            {
                return BadRequest(new { message = "Order data is invalid or ID mismatch" });
            }

            var response = await _orderService.UpdateOrderAsync(orderDto);
            return response.Status == ResponseStatus.Success ? Ok(response.Data) : NotFound(new { message = response.Message });
        }

        // DELETE: api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _orderService.DeleteOrderAsync(id);
            return response.Status == ResponseStatus.Success ? (response.Data ? Ok() : NotFound(new { message = response.Message })) : StatusCode(500, new { message = response.Message });
        }
    }
}