using HmsNet.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HmsNet.UI.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(IHttpClientFactory httpClientFactory, ILogger<TransactionsController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IActionResult> Index()
        {
            var viewModel = new List<BillDto>();
            
            try
            {
                var response = await _httpClient.GetAsync($"api/Bills");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<IEnumerable<BillDto>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (serviceResponse?.Status == 0 && serviceResponse.Data != null)
                {
                    viewModel.AddRange(serviceResponse.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = serviceResponse?.Message ?? "Failed to load transactions.";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching transactions: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Unable to connect to the API.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An unexpected error occurred while loading transactions.";
            }

            if (TempData.ContainsKey("SuccessMessage"))
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            if (TempData.ContainsKey("ErrorMessage"))
                ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(viewModel);
        }
    }
}
