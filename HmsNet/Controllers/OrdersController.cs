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

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<IEnumerable<OrderDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeDetails = false)
        {
            var response = await _orderService.GetAllOrdersAsync(page, pageSize, includeDetails);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : StatusCode(StatusCodes.Status500InternalServerError, response);
        }


        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<OrderDto>>> GetById(int id, [FromQuery]bool includeDetails = false)
        {
            var response = await _orderService.GetOrderByIdAsync(id, includeDetails);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : NotFound(response);
        }

        // GET: api/Orders/room/5
        [HttpGet("{roomId}")]
        public async Task<ActionResult<ServiceResponse<OrderDto>>> GetOrdersByRoomId(int roomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeDetails = false)
        {
            var response = await _orderService.GetOrdersByRoomIdAsync(roomId, page, pageSize, includeDetails);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : NotFound(response);
        }

        // GET: api/Orders/5/total
        [HttpGet("{id}/total")]
        public async Task<ActionResult<ServiceResponse<OrderDto>>> GetTotalAmount(int id)
        {
            var response = await _orderService.CalculateTotalAmountAsync(id);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : NotFound(response);
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<OrderDto>>> Create(OrderDto orderDto)
        {
            var response = await _orderService.CreateOrderAsync(orderDto);
            return response.Status == ResponseStatus.Success
                ? CreatedAtAction(nameof(GetById), new { id = response.Data.OrderId }, response)
                : BadRequest(response);
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<OrderDto>>> UpdateItem(int id, OrderDto orderDto)
        {
            if (id != orderDto.OrderId)
            {
                var res = new ServiceResponse<OrderDto>
                {
                    Status = ResponseStatus.Error,
                    Message = "Order ID mismatch",
                    Data = null
                };
                return BadRequest(res);
            }

            var response = await _orderService.UpdateOrderAsync(orderDto);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : response.Message.Contains("not found")
                    ? NotFound(response)
                    : BadRequest(response);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<bool>>> Delete(int id)
        {
            var response = await _orderService.DeleteOrderAsync(id);
            return response.Status == ResponseStatus.Success
                ? NoContent()
                : response.Message.Contains("not found")
                    ? NotFound(response)
                    : BadRequest(response);
        }
    }
}