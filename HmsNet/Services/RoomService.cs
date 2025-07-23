using HmsNet.Data;
using HmsNet.Models.Domain;
using HmsNet.Models.DTO;
using HmsNet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HmsNet.Services
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<IEnumerable<Room>>> GetAllAsync()
        {
            var response = new ServiceResponse<IEnumerable<Room>>();
            try
            {
                response.Data = await _context.Rooms.ToListAsync();
                return response;
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Message = $"Error retrieving rooms: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<Room>> GetByIdAsync(int id)
        {
            var response = new ServiceResponse<Room>();
            try
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    response.Status = "Error";
                    response.Message = $"Room with ID {id} not found";
                    return response;
                }
                response.Data = room;
                return response;
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Message = $"Error retrieving room with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<Room>> CreateAsync(Room room)
        {
            var response = new ServiceResponse<Room>();
            try
            {
                if (room == null)
                {
                    response.Status = "Error";
                    response.Message = "Room cannot be null";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(room.RoomName))
                {
                    response.Status = "Error";
                    response.Message = "Room name is required";
                    return response;
                }

                if (room.Capacity <= 0)
                {
                    response.Status = "Error";
                    response.Message = "Capacity must be greater than zero";
                    return response;
                }

                if (_context.Rooms.Any(r => r.RoomName == room.RoomName))
                {
                    response.Status = "Error";
                    response.Message = "Room with this name already exists";
                    return response;
                }

                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                response.Data = room;
                return response;
            }
            catch (DbUpdateException ex)
            {
                response.Status = "Error";
                response.Message = $"Error creating room: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<Room>> UpdateAsync(Room room)
        {
            var response = new ServiceResponse<Room>();
            try
            {
                if (room == null)
                {
                    response.Status = "Error";
                    response.Message = "Room cannot be null";
                    return response;
                }

                var existingRoom = await _context.Rooms.FindAsync(room.RoomId);
                if (existingRoom == null)
                {
                    response.Status = "Error";
                    response.Message = $"Room with ID {room.RoomId} not found";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(room.RoomName))
                {
                    response.Status = "Error";
                    response.Message = "Room name is required";
                    return response;
                }

                if (room.Capacity <= 0)
                {
                    response.Status = "Error";
                    response.Message = "Capacity must be greater than zero";
                    return response;
                }

                existingRoom.RoomName = room.RoomName;
                existingRoom.RoomType = room.RoomType;
                existingRoom.Capacity = room.Capacity;
                existingRoom.Status = room.Status;

                await _context.SaveChangesAsync();
                response.Data = existingRoom;
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                response.Status = "Error";
                response.Message = $"Concurrency error updating room with ID {room.RoomId}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (DbUpdateException ex)
            {
                response.Status = "Error";
                response.Message = $"Error updating room with ID {room.RoomId}: {ex.Message}";
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
                    response.Status = "Error";
                    response.Message = $"Room with ID {id} not found";
                    response.Data = false;
                    return response;
                }

                if (_context.Orders.Any(o => o.RoomId == id))
                {
                    response.Status = "Error";
                    response.Message = "Cannot delete room with active orders";
                    response.Data = false;
                    return response;
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                response.Data = true;
                return response;
            }
            catch (DbUpdateException ex)
            {
                response.Status = "Error";
                response.Message = $"Error deleting room with ID {id}: {ex.Message}";
                response.Data = false;
                return response;
            }
        }
    }
}
