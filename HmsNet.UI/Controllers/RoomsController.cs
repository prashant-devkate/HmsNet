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
    public class RoomsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RoomsController> _logger;

        public RoomsController(IHttpClientFactory httpClientFactory, ILogger<RoomsController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var viewModel = new RoomListViewModel
            {
                Rooms = new List<RoomDto>(),
                CurrentPage = page,
                PageSize = pageSize
            };

            try
            {
                var query = $"?page={page}&pageSize={pageSize}";
                var response = await _httpClient.GetAsync($"api/Rooms{query}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<IEnumerable<RoomDto>>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (serviceResponse?.Status == 0 && serviceResponse.Data != null)
                {
                    viewModel.Rooms.AddRange(serviceResponse.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = serviceResponse?.Message ?? "Failed to load rooms.";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching rooms: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Unable to connect to the API.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching rooms: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An unexpected error occurred while loading rooms.";
            }

            if (TempData.ContainsKey("SuccessMessage"))
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            if (TempData.ContainsKey("ErrorMessage"))
                ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
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

        [HttpPost("Delete/Room/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var httpResponseMessage = await _httpClient.DeleteAsync($"api/Rooms/{id}");

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "Failed to delete room. " + content;
                return RedirectToAction("Index", "Rooms");
            }

            TempData["SuccessMessage"] = "Room deleted successfully.";
            return RedirectToAction("Index", "Rooms");
        }

        [HttpGet("Edit/Room/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Rooms/{id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API response content for Edit GET: {Content}", json);

                var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<RoomDto>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (serviceResponse?.Status == 0 && serviceResponse.Data != null)
                {
                    return View(serviceResponse.Data);
                }
                else
                {
                    _logger.LogWarning("Room with ID {Id} not found or failed to load.", id);
                    TempData["ErrorMessage"] = serviceResponse?.Message ?? "Room not found.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching room {Id}: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = "Unable to connect to the API.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching room {Id}: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = "An unexpected error occurred while loading the room.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("Edit/Room/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomDto roomDto)
        {
            if (id != roomDto.RoomId)
            {
                _logger.LogWarning("Room ID mismatch: URL ID {Id} does not match form ID {FormId}", id, roomDto.RoomId);
                ModelState.AddModelError(string.Empty, "Room ID mismatch.");
                return View(roomDto);
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for Edit: {Errors}", ModelState);
                return View(roomDto);
            }

            try
            {
                var json = JsonSerializer.Serialize(roomDto);
                _logger.LogInformation("Sending JSON payload to api/Rooms/{Id}: {Json}", id, json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/Rooms/{id}", content);
                _logger.LogInformation("API response status for Edit POST: {StatusCode}", response.StatusCode);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API response content for Edit POST: {Content}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Non-success status code received: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, $"API error: {responseContent}");
                    return View(roomDto);
                }

                var serviceResponse = JsonSerializer.Deserialize<ServiceResponse<RoomDto>>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (serviceResponse?.Status == 0)
                {
                    TempData["SuccessMessage"] = "Room updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, serviceResponse?.Message ?? "Failed to update room.");
                    return View(roomDto);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while updating room {Id}: {Message} (Status: {StatusCode})", id, ex.Message, ex.StatusCode);
                ModelState.AddModelError(string.Empty, "Unable to connect to the API.");
                return View(roomDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room {Id}: {Message}", id, ex.Message);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the room.");
                return View(roomDto);
            }
        }
    }
}