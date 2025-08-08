using HmsNet.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HmsNet.UI.Controllers
{
    public class ItemsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(IHttpClientFactory httpClientFactory, ILogger<ItemsController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new ItemListViewModel
            {
                Items = new List<ItemDto>()
            };

            try
            {
                var response = await _httpClient.GetAsync($"api/Items");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<IEnumerable<ItemDto>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (serviceResponse?.Status == 0 && serviceResponse.Data != null)
                {
                    viewModel.Items.AddRange(serviceResponse.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = serviceResponse?.Message ?? "Failed to load items.";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching items: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Unable to connect to the API.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching items: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An unexpected error occurred while loading items.";
            }

            if (TempData.ContainsKey("SuccessMessage"))
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            if (TempData.ContainsKey("ErrorMessage"))
                ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(viewModel);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new ItemDto { IsActive = true }); // Default IsActive to true
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Create: {Errors}", ModelState);
                return View(itemDto);
            }

            // Validate category (example fix for "Fod" typo)
            if (string.IsNullOrWhiteSpace(itemDto.Category) || !new[] { "Food", "Drink", "Dessert" }.Contains(itemDto.Category))
            {
                ModelState.AddModelError("itemDto.Category", "Category must be 'Food', 'Drink', or 'Dessert'.");
                return View(itemDto);
            }

            try
            {
                var json = JsonSerializer.Serialize(itemDto);
                _logger.LogInformation("Sending JSON payload to api/Items: {Json}", json); // Log the request payload
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/Items", content);
                _logger.LogInformation("API response status for Create: {StatusCode}", response.StatusCode); // Log status code

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API response content: {Content}", responseContent); // Log full response

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Non-success status code received: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, $"API error: {responseContent}");
                    return View(itemDto);
                }

                var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<ItemDto>>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (serviceResponse?.Status == 0)
                {
                    TempData["SuccessMessage"] = "Item created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, serviceResponse?.Message ?? "Failed to create item.");
                    return View(itemDto);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while creating item: {Message} (Status: {StatusCode})", ex.Message, ex.StatusCode);
                ModelState.AddModelError(string.Empty, "Unable to connect to the API.");
                return View(itemDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the item.");
                return View(itemDto);
            }
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Items/{id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API response content for Edit GET: {Content}", json);

                var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<ItemDto>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (serviceResponse?.Status == 0 && serviceResponse.Data != null)
                {
                    return View(serviceResponse.Data);
                }
                else
                {
                    _logger.LogWarning("Item with ID {Id} not found or failed to load.", id);
                    TempData["ErrorMessage"] = serviceResponse?.Message ?? "Item not found.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching item {Id}: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = "Unable to connect to the API.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching item {Id}: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = "An unexpected error occurred while loading the item.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemDto itemDto)
        {
            if (id != itemDto.ItemId)
            {
                _logger.LogWarning("Item ID mismatch: URL ID {Id} does not match form ID {FormId}", id, itemDto.ItemId);
                ModelState.AddModelError(string.Empty, "Item ID mismatch.");
                return View(itemDto);
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Edit: {Errors}", ModelState);
                return View(itemDto);
            }

            // Validate category
            if (string.IsNullOrWhiteSpace(itemDto.Category) || !new[] { "Food", "Drink", "Dessert" }.Contains(itemDto.Category))
            {
                ModelState.AddModelError("itemDto.Category", "Category must be 'Food', 'Drink', or 'Dessert'.");
                return View(itemDto);
            }

            try
            {
                var json = JsonSerializer.Serialize(itemDto);
                _logger.LogInformation("Sending JSON payload to api/Items/{Id}: {Json}", id, json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/Items/{id}", content);
                _logger.LogInformation("API response status for Edit POST: {StatusCode}", response.StatusCode);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API response content for Edit POST: {Content}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Non-success status code received: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, $"API error: {responseContent}");
                    return View(itemDto);
                }

                var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<ItemDto>>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (serviceResponse?.Status == 0)
                {
                    TempData["SuccessMessage"] = "Item updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, serviceResponse?.Message ?? "Failed to update item.");
                    return View(itemDto);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while updating item {Id}: {Message} (Status: {StatusCode})", id, ex.Message, ex.StatusCode);
                ModelState.AddModelError(string.Empty, "Unable to connect to the API.");
                return View(itemDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {Id}: {Message}", id, ex.Message);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the item.");
                return View(itemDto);
            }
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var httpResponseMessage = await _httpClient.DeleteAsync($"api/Items/{id}");

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "Failed to delete item. " + content;
                return RedirectToAction("Index", "Items");
            }

            TempData["SuccessMessage"] = "Item deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}