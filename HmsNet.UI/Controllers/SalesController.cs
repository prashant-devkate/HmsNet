using HmsNet.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;

namespace HmsNet.UI.Controllers
{
    public class SalesController : Controller
    {
        private readonly HttpClient _httpClient;

        public SalesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {

            ViewBag.ApiBaseUrl = _httpClient.BaseAddress?.ToString() ?? "https://localhost:7165/"; // Fallback

            var viewModel = new ItemListViewModel
            {
                Items = new List<ItemDto>(),
                CurrentPage = page,
                PageSize = pageSize
            };

                var query = $"?page={page}&pageSize={pageSize}";
                var response = await _httpClient.GetAsync($"api/Items{query}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<IEnumerable<ItemDto>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (serviceResponse?.Status == 0 && serviceResponse.Data != null)
                {
                    viewModel.Items.AddRange(serviceResponse.Data);
                }

            return View(viewModel);

        }
    }
}
