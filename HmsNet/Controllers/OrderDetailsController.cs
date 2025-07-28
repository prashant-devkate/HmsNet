using HmsNet.Enums;
using HmsNet.Models.DTO;
using HmsNet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HmsNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly IOrderDetailService _orderService;

        public OrderDetailsController(IOrderDetailService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        // GET: api/OrderDetails
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<IEnumerable<OrderDetailDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeDetails = false)
        {
            var response = await _orderService.GetAllOrderDetailsAsync(page, pageSize, includeDetails);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : StatusCode(StatusCodes.Status500InternalServerError, response);
        }


        // GET: api/OrderDetails/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<OrderDetailDto>>> GetById(int id, [FromQuery] bool includeDetails = false)
        {
            var response = await _orderService.GetOrderDetailsByIdAsync(id, includeDetails);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : NotFound(response);
        }

        // POST: api/OrderDetails
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<OrderDetailDto>>> Create(OrderDetailDto orderDto)
        {
            var response = await _orderService.CreateOrderDetailsAsync(orderDto);
            return response.Status == ResponseStatus.Success
                ? CreatedAtAction(nameof(GetById), new { id = response.Data.OrderDetailId }, response)
                : BadRequest(response);
        }

        // PUT: api/OrderDetails/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<OrderDetailDto>>> UpdateItem(int id, OrderDetailDto orderDto)
        {
            if (id != orderDto.OrderDetailId)
            {
                var res = new ServiceResponse<OrderDetailDto>
                {
                    Status = ResponseStatus.Error,
                    Message = "Order detail ID mismatch",
                    Data = null
                };
                return BadRequest(res);
            }

            var response = await _orderService.UpdateOrderDetailsAsync(orderDto);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : response.Message.Contains("not found")
                    ? NotFound(response)
                    : BadRequest(response);
        }

        // DELETE: api/OrderDetails/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<bool>>> Delete(int id)
        {
            var response = await _orderService.DeleteOrderDetailsAsync(id);
            return response.Status == ResponseStatus.Success
                ? NoContent()
                : response.Message.Contains("not found")
                    ? NotFound(response)
                    : BadRequest(response);
        }
    }
}