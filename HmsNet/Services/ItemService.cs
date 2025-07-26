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
   
    public class ItemService : IItemService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ItemService> _logger;
        private const int MaxNameLength = 100;
        private const int MaxCategoryLength = 50;

        public ItemService(AppDbContext context, ILogger<ItemService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool ValidateItemDto(ItemDto item, out string errorMessage)
        {
            if (item == null)
            {
                errorMessage = "Item cannot be null";
                return false;
            }
            if (string.IsNullOrWhiteSpace(item.ItemName))
            {
                errorMessage = "Item name is required";
                return false;
            }
            if (item.ItemName.Length > MaxNameLength)
            {
                errorMessage = $"Item name cannot exceed {MaxNameLength} characters";
                return false;
            }
            if (string.IsNullOrWhiteSpace(item.Category))
            {
                errorMessage = "Item category is required";
                return false;
            }
            if (item.Category.Length > MaxCategoryLength)
            {
                errorMessage = $"Category cannot exceed {MaxCategoryLength} characters";
                return false;
            }
            if (item.Rate <= 0)
            {
                errorMessage = "Rate must be greater than zero";
                return false;
            }
            errorMessage = null;
            return true;
        }

        private Item MapToItem(ItemDto dto)
        {
            return new Item
            {
                ItemId = dto.ItemId,
                ItemName = dto.ItemName?.Trim(),
                Category = dto.Category?.Trim(),
                Rate = dto.Rate,
                IsActive = dto.IsActive
            };
        }

        private ItemDto MapToItemDto(Item item)
        {
            return new ItemDto
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                Category = item.Category,
                Rate = item.Rate,
                IsActive = item.IsActive
            };
        }

        public async Task<ServiceResponse<IEnumerable<ItemDto>>> GetAllAsync(int page = 1, int pageSize = 10, bool includeOrderDetails = false)
        {
            var response = new ServiceResponse<IEnumerable<ItemDto>>();
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Invalid page or pageSize";
                    return response;
                }

                var query = _context.Items.AsQueryable();
                if (includeOrderDetails)
                {
                    query = query.Include(i => i.OrderDetails);
                }

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                response.Data = items.Select(MapToItemDto).ToList();
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving items: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving items: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<IEnumerable<ItemDto>>> GetAllActiveAsync(int page = 1, int pageSize = 10, bool includeOrderDetails = false)
        {
            var response = new ServiceResponse<IEnumerable<ItemDto>>();
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Invalid page or pageSize";
                    return response;
                }

                var query = _context.Items.AsQueryable();
                if (includeOrderDetails)
                {
                    query = query.Include(i => i.OrderDetails);
                }
                query = query.Where(i => i.IsActive);

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                response.Data = items.Select(MapToItemDto).ToList();
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving items: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving items: {ex.Message}";
                response.Data = null;
                return response;
            }
        }


        public async Task<ServiceResponse<ItemDto>> GetByIdAsync(int id, bool includeOrderDetails = false)
        {
            var response = new ServiceResponse<ItemDto>();
            try
            {
                var query = _context.Items.AsQueryable();
                if (includeOrderDetails)
                {
                    query = query.Include(i => i.OrderDetails);
                }

                var item = await query.FirstOrDefaultAsync(i => i.ItemId == id);
                if (item == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Item with ID {id} not found";
                    return response;
                }

                response.Data = MapToItemDto(item);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving item with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<ItemDto>> CreateAsync(ItemDto itemDto)
        {
            var response = new ServiceResponse<ItemDto>();
            try
            {
                if (!ValidateItemDto(itemDto, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                if (await _context.Items.AnyAsync(r => r.ItemName.Trim().ToLower() == itemDto.ItemName.Trim().ToLower()))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Item with this name already exists";
                    return response;
                }

                var item = MapToItem(itemDto);
                item.IsActive = true;

                await using var transaction = await _context.Database.BeginTransactionAsync();
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = MapToItemDto(item);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating item: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error creating item: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<ItemDto>> UpdateAsync(ItemDto itemDto)
        {
            var response = new ServiceResponse<ItemDto>();
            try
            {
                if (!ValidateItemDto(itemDto, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                var existingItem = await _context.Items.FindAsync(itemDto.ItemId);
                if (existingItem == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Item with ID {itemDto.ItemId} not found";
                    return response;
                }

                if (await _context.Items.AnyAsync(r => r.ItemName.Trim().ToLower() == itemDto.ItemName.Trim().ToLower() && r.ItemId != itemDto.ItemId))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Another item with this name already exists";
                    return response;
                }

                existingItem.ItemName = itemDto.ItemName.Trim();
                existingItem.Category = itemDto.Category.Trim();
                existingItem.Rate = itemDto.Rate;
                existingItem.IsActive = itemDto.IsActive;

                await using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = MapToItemDto(existingItem);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating item with ID {Id}: {Message}", itemDto.ItemId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Concurrency error updating item with ID {itemDto.ItemId}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating item with ID {Id}: {Message}", itemDto.ItemId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error updating item with ID {itemDto.ItemId}: {ex.Message}";
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
                if (item == null || !item.IsActive)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Item with ID {id} not found";
                    response.Data = false;
                    return response;
                }

                if (await _context.OrderDetails.AnyAsync(o => o.ItemId == id))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Cannot delete item with active orders";
                    response.Data = false;
                    return response;
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();
                
                _context.Items.Remove(item);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = true;
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting item with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error deleting item with ID {id}: {ex.Message}";
                response.Data = false;
                return response;
            }
        }
    }
}