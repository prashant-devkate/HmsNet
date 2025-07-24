using HmsNet.Data;
using HmsNet.Models.Domain;
using HmsNet.Models.DTO;
using HmsNet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HmsNet.Services
{
    public class ItemService : IItemService
    {
        private readonly AppDbContext _context;

        public ItemService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<IEnumerable<Item>>> GetAllAsync()
        {
            var response = new ServiceResponse<IEnumerable<Item>>();
            try
            {
                response.Data = await _context.Items.ToListAsync();
                return response;
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Message = $"Error retrieving items: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<Item>> GetByIdAsync(int id)
        {
            var response = new ServiceResponse<Item>();
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null)
                {
                    response.Status = "Error";
                    response.Message = $"Item with ID {id} not found";
                    return response;
                }
                response.Data = item;
                return response;
            }
            catch (Exception ex)
            {
                response.Status = "Error";
                response.Message = $"Error retrieving item with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<Item>> CreateAsync(Item item)
        {
            var response = new ServiceResponse<Item>();
            try
            {
                if (item == null)
                {
                    response.Status = "Error";
                    response.Message = "Item cannot be null";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(item.ItemName))
                {
                    response.Status = "Error";
                    response.Message = "Item name is required";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(item.Category))
                {
                    response.Status = "Error";
                    response.Message = "Item category is required";
                    return response;
                }

                if (item.Rate <= 0)
                {
                    response.Status = "Error";
                    response.Message = "Rate must be greater than zero";
                    return response;
                }

                if (_context.Items.Any(r => r.ItemName == item.ItemName))
                {
                    response.Status = "Error";
                    response.Message = "Item with this name already exists";
                    return response;
                }

                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                response.Data = item;
                return response;
            }
            catch (DbUpdateException ex)
            {
                response.Status = "Error";
                response.Message = $"Error creating item: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<Item>> UpdateAsync(Item item)
        {
            var response = new ServiceResponse<Item>();
            try
            {
                if (item == null)
                {
                    response.Status = "Error";
                    response.Message = "Item cannot be null";
                    return response;
                }

                var existingItem = await _context.Items.FindAsync(item.ItemId);
                if (existingItem == null)
                {
                    response.Status = "Error";
                    response.Message = $"Item with ID {item.ItemId} not found";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(item.ItemName))
                {
                    response.Status = "Error";
                    response.Message = "Item name is required";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(item.Category))
                {
                    response.Status = "Error";
                    response.Message = "Item category is required";
                    return response;
                }

                if (item.Rate <= 0)
                {
                    response.Status = "Error";
                    response.Message = "Rate must be greater than zero";
                    return response;
                }

                existingItem.ItemName = item.ItemName;
                existingItem.Category = item.Category;
                existingItem.Rate = item.Rate;
                existingItem.IsActive = item.IsActive;

                await _context.SaveChangesAsync();
                response.Data = existingItem;
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                response.Status = "Error";
                response.Message = $"Concurrency error updating item with ID {item.ItemId}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (DbUpdateException ex)
            {
                response.Status = "Error";
                response.Message = $"Error updating item with ID {item.ItemId}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null)
                {
                    response.Status = "Error";
                    response.Message = $"Item with ID {id} not found";
                    response.Data = false;
                    return response;
                }

                if (_context.OrderDetails.Any(o => o.ItemId == id))
                {
                    response.Status = "Error";
                    response.Message = "Cannot delete item with active orders";
                    response.Data = false;
                    return response;
                }

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                response.Data = true;
                return response;
            }
            catch (DbUpdateException ex)
            {
                response.Status = "Error";
                response.Message = $"Error deleting item with ID {id}: {ex.Message}";
                response.Data = false;
                return response;
            }
        }
    }
}
