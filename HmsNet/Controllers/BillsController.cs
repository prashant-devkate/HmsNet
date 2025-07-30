using HmsNet.Enums;
using HmsNet.Models.DTO;
using HmsNet.Services;
using HmsNet.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HmsNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillsController : ControllerBase
    {
        private readonly IBillService _service;

        public BillsController(IBillService service)
        {
            _service = service;
        }

        // GET: api/Bills
        [HttpGet]
        public async Task<ActionResult<ServiceResponse<IEnumerable<BillDto>>>> GetBills(int page = 1, int pageSize = 10)
        {
            var response = await _service.GetAllAsync(page, pageSize);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : StatusCode(StatusCodes.Status500InternalServerError, response);
        }


        // GET: api/Bills/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<BillDto>>> GetBill(int id)
        {
            var response = await _service.GetByIdAsync(id);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : NotFound(response);
        }

        // POST: api/Bills
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<BillDto>>> CreateBill(BillDto billDto)
        {
            var response = await _service.CreateAsync(billDto);
            return response.Status == ResponseStatus.Success
                ? CreatedAtAction(nameof(GetBill), new { id = response.Data.BillId }, response)
                : BadRequest(response);
        }

        // PUT: api/Bills/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<BillDto>>> UpdateRoom(int id, BillDto billDto)
        {
            if (id != billDto.BillId)
            {
                var res = new ServiceResponse<BillDto>
                {
                    Status = ResponseStatus.Error,
                    Message = "Bill ID mismatch",
                    Data = null
                };
                return BadRequest(res);
            }

            var response = await _service.UpdateAsync(billDto);
            return response.Status == ResponseStatus.Success
                ? Ok(response)
                : response.Message.Contains("not found")
                    ? NotFound(response)
                    : BadRequest(response);
        }

        // DELETE: api/Bills/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteBill(int id)
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