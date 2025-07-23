using HmsNet.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace HmsNet.UI.Controllers
{
    public class RoomsController : Controller
    {
        private readonly HttpClient _httpClient;

        public RoomsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }
        public IActionResult Create()
        {
            var model = new RoomDto
            {
                Orders = new List<OrderDto> { new OrderDto { OrderDetails = new List<OrderDetailDto>(), Bill = new BillDto { Transactions = new List<TransactionDto>() } } }
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoomDto roomDto)
        {
            if (!ModelState.IsValid)
            {
                return View(roomDto);
            }

            // Set current date and time (06:52 PM IST, July 23, 2025)
            var currentDateTime = new DateTime(2025, 7, 23, 18, 52, 0, DateTimeKind.Utc);
            if (roomDto.Orders != null)
            {
                foreach (var order in roomDto.Orders)
                {
                    order.OrderDateTime = currentDateTime;
                    if (order.OrderDetails != null)
                    {
                        foreach (var detail in order.OrderDetails)
                        {
                            detail.Amount = detail.Quantity * detail.Rate;
                        }
                    }
                    if (order.Bill != null)
                    {
                        order.Bill.BillDateTime = currentDateTime;
                        order.Bill.TotalAmount = order.OrderDetails?.Sum(d => d.Amount) ?? 0;
                        order.Bill.FinalAmount = order.Bill.TotalAmount - order.Bill.DiscountAmount + order.Bill.TaxAmount;
                        if (order.Bill.Transactions != null)
                        {
                            foreach (var transaction in order.Bill.Transactions)
                            {
                                transaction.TransactionDateTime = currentDateTime;
                            }
                        }
                    }
                    order.TotalAmount = order.OrderDetails?.Sum(d => d.Amount) ?? 0;
                }
            }

            var response = await _httpClient.PostAsJsonAsync("api/Rooms", roomDto);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<RoomDto>>(responseContent);
                ViewBag.Message = "Room created successfully!";
                return View("Create", new RoomDto());
            }
            else
            {
                ViewBag.Message = "Error creating room. Please check the data and try again.";
                return View(roomDto);
            }
        }
    }
}
