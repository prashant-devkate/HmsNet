using HmsNet.Data;
using HmsNet.Enums;
using HmsNet.Models.Domain;
using HmsNet.Models.DTO;
using HmsNet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HmsNet.Services
{

    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoomService> _logger;
        private const int MaxNameLength = 100;
        private const int MaxCategoryLength = 50;

        public RoomService(AppDbContext context, ILogger<RoomService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool ValidateRoomDto(RoomDto item, out string errorMessage)
        {
            if (item == null)
            {
                errorMessage = "Room cannot be null";
                return false;
            }
            if (string.IsNullOrWhiteSpace(item.RoomName))
            {
                errorMessage = "Room name is required";
                return false;
            }
            if (item.RoomName.Length > MaxNameLength)
            {
                errorMessage = $"Room name cannot exceed {MaxNameLength} characters";
                return false;
            }
            if (string.IsNullOrWhiteSpace(item.RoomType))
            {
                errorMessage = "Room type is required";
                return false;
            }
            if (item.Capacity <= 0)
            {
                errorMessage = "Capacity must be greater than zero";
                return false;
            }
            errorMessage = null;
            return true;
        }

        private bool ValidateStatus(string status, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                errorMessage = "Status is required";
                return false;
            }
            if (status != "Available" && status != "Pending")
            {
                errorMessage = "Status must be either 'Available' or 'Pending'";
                return false;
            }
            errorMessage = null;
            return true;
        }

        private bool ValidateOrderId(int orderId, out string errorMessage)
        {
            if (orderId <= 0)
            {
                errorMessage = "Invalid order id";
                return false;
            }
            errorMessage = null;
            return true;
        }

        private Room MapToRoom(RoomDto dto)
        {
            return new Room
            {
                RoomId = dto.RoomId,
                RoomName = dto.RoomName?.Trim(),
                RoomType = dto.RoomType?.Trim(),
                Capacity = dto.Capacity,
                Status = dto.Status,
                OrderId = dto.OrderId
            };
        }

        private RoomDto MapToRoomDto(Room room)
        {
            return new RoomDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                Status = room.Status,
                OrderId = room.OrderId
            };
        }

        public async Task<ServiceResponse<IEnumerable<RoomDto>>> GetAllActiveAsync(int page = 1, int pageSize = 10, bool includeOrders = false)
        {
            var response = new ServiceResponse<IEnumerable<RoomDto>>();
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Invalid page or pageSize";
                    return response;
                }

                var query = _context.Rooms.AsQueryable();
                if (includeOrders)
                {
                    query = query.Include(i => i.Orders);
                }
                query = query.Where(i => i.Status == "Available" ); 

                var rooms = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                response.Data = rooms.Select(MapToRoomDto).ToList();
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving rooms: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<IEnumerable<RoomDto>>> GetAllAsync(int page = 1, int pageSize = 10, bool includeOrders = false)
        {
            var response = new ServiceResponse<IEnumerable<RoomDto>>();
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Invalid page or pageSize";
                    return response;
                }

                var query = _context.Rooms.AsQueryable();
                if (includeOrders)
                {
                    query = query.Include(i => i.Orders);
                }

                var rooms = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RoomDto
                    {
                        RoomId = r.RoomId,
                        RoomName = r.RoomName,
                        RoomType = r.RoomType,
                        Capacity = r.Capacity,
                        Status = r.Status,
                        OrderId = _context.Orders
                    .Where(o => o.RoomId == r.RoomId && o.Status == "Pending")
                    .Select(o => (int?)o.OrderId)
                    .FirstOrDefault()
                    }).ToListAsync();

                response.Data = rooms;
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rooms: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving rooms: {ex.Message}";
                response.Data = null;
                return response;
            }
        }


        public async Task<ServiceResponse<RoomDto>> GetByIdAsync(int id, bool includeOrders = false)
        {
            var response = new ServiceResponse<RoomDto>();
            try
            {
                var query = _context.Rooms.AsQueryable();
                if (includeOrders)
                {
                    query = query.Include(i => i.Orders);
                }

                var room = await query.FirstOrDefaultAsync(i => i.RoomId == id);
                if (room == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Room with ID {id} not found";
                    return response;
                }

                response.Data = MapToRoomDto(room);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving room with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving room with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<RoomDto>> CreateAsync(RoomDto roomDto)
        {
            var response = new ServiceResponse<RoomDto>();
            try
            {
                if (!ValidateRoomDto(roomDto, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                if (await _context.Rooms.AnyAsync(r => r.RoomType == roomDto.RoomType && r.RoomName.Trim().ToLower() == roomDto.RoomName.Trim().ToLower()))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Room with this name already exists in the {roomDto.RoomType}";
                    return response;
                }

                var room = MapToRoom(roomDto);
                room.Status = "Available"; // Ensure new rooms are available

                await using var transaction = await _context.Database.BeginTransactionAsync();
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = MapToRoomDto(room);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating room: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error creating room: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<RoomDto>> UpdateAsync(RoomDto roomDto)
        {
            var response = new ServiceResponse<RoomDto>();
            try
            {
                if (!ValidateRoomDto(roomDto, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                var existingRoom = await _context.Rooms.FindAsync(roomDto.RoomId);
                if (existingRoom == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Room with ID {roomDto.RoomId} not found";
                    return response;
                }

                if (await _context.Rooms.AnyAsync(r => r.RoomType == roomDto.RoomType && r.RoomName.Trim().ToLower() == roomDto.RoomName.Trim().ToLower() && r.RoomId != roomDto.RoomId))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Another room with this name already exists in {roomDto.RoomType}";
                    return response;
                }

                existingRoom.RoomName = roomDto.RoomName.Trim();
                existingRoom.RoomType = roomDto.RoomType.Trim();
                existingRoom.Capacity = roomDto.Capacity;
                existingRoom.Status = roomDto.Status;

                await using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = MapToRoomDto(existingRoom);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating room with ID {Id}: {Message}", roomDto.RoomId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Concurrency error updating room with ID {roomDto.RoomId}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating room with ID {Id}: {Message}", roomDto.RoomId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error updating room with ID {roomDto.RoomId}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Room with ID {id} not found";
                    response.Data = false;
                    return response;
                }

                if (await _context.Orders.AnyAsync(o => o.RoomId == id))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Cannot delete room with active orders";
                    response.Data = false;
                    return response;
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();
               
                _context.Rooms.Remove(room);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = true;
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting room with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error deleting room with ID {id}: {ex.Message}";
                response.Data = false;
                return response;
            }
        }

        public async Task<ServiceResponse<RoomDto>> UpdateStatusAsync(int id, string status)
        {
            var response = new ServiceResponse<RoomDto>();
            try
            {
                // Validate the status
                if (!ValidateStatus(status, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                // Find the room
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Room with ID {id} not found";
                    return response;
                }

                // Update the status
                room.Status = status.Trim();

                // Save changes with transaction
                await using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return the updated room
                response.Data = MapToRoomDto(room);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating status for room with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Concurrency error updating status for room with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating status for room with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error updating status for room with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating status for room with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Unexpected error updating status for room with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<RoomDto>> UpdateOrderIdAsync(int id, int orderId)
        {
            var response = new ServiceResponse<RoomDto>();
            try
            {
                // Validate the order id
                if (!ValidateOrderId(orderId, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                // Find the room
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Room with ID {id} not found";
                    return response;
                }

                // Update the order id
                room.OrderId = orderId;

                // Save changes with transaction
                await using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return the updated room
                response.Data = MapToRoomDto(room);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating order id for room with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Concurrency error updating order id for room with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating order id for room with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error updating order id for room with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating order id for room with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Unexpected error updating order id for room with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }
    }
}