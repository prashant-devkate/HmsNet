using HmsNet.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HmsNet.UI.Controllers
{
    public class SalesController : Controller
    {
        private readonly HttpClient _httpClient;

        public SalesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 50)
        {
            ViewBag.ApiBaseUrl = _httpClient.BaseAddress?.ToString() ?? "https://localhost:7165/";

            var viewModel = new ItemListViewModel
            {
                Items = new List<ItemDto>(),
                RoomsByType = new Dictionary<string, List<RoomDto>>(),
                CurrentPage = page,
                PageSize = pageSize
            };

            var query = $"?page={page}&pageSize={pageSize}";
            var response = await _httpClient.GetAsync($"api/Items/Active{query}");
            var roomResponse = await _httpClient.GetAsync($"api/Rooms{query}");

            response.EnsureSuccessStatusCode();
            roomResponse.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var roomJson = await roomResponse.Content.ReadAsStringAsync();

            var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<IEnumerable<ItemDto>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var roomServiceResponse = JsonSerializer.Deserialize<ServiceResponse<IEnumerable<RoomDto>>>(roomJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (serviceResponse?.Status == 0 && serviceResponse.Data != null)
            {
                viewModel.Items.AddRange(serviceResponse.Data);
            }

            if (roomServiceResponse?.Status == 0 && roomServiceResponse.Data != null)
            {
                // Group rooms by roomType
                viewModel.RoomsByType = roomServiceResponse.Data
                    .GroupBy(r => r.RoomType)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy(r => r.RoomName).ToList()
                    );
            }

            return View(viewModel);
        }
    }
}