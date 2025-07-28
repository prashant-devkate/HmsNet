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
    public class OrderDetailService : IOrderDetailService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderDetailService> _logger;

        public OrderDetailService(AppDbContext context, ILogger<OrderDetailService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool ValidateOrderDetailDto(OrderDetailDto order, out string errorMessage)
        {
            if (order == null)
            {
                errorMessage = "Order detail cannot be null";
                return false;
            }
            if (order.OrderId <= 0)
            {
                errorMessage = "Order ID must be greater than zero";
                return false;
            }
            if (order.ItemId <= 0)
            {
                errorMessage = "Item ID must be greater than zero";
                return false;
            }
            if (order.Quantity <= 0)
            {
                errorMessage = "Quantity must be greater than zero";
                return false;
            }
            if (order.Rate <= 0)
            {
                errorMessage = "Rate must be greater than zero";
                return false;
            }
            if (order.Amount <= 0)
            {
                errorMessage = "Amount must be greater than zero";
                return false;
            }
            errorMessage = null;
            return true;
        }

        private OrderDetail MapToOrderDetail(OrderDetailDto dto)
        {
            return new OrderDetail
            {
                OrderId = dto.OrderId,
                ItemId = dto.ItemId,
                Quantity = dto.Quantity,
                Rate = dto.Rate,
                Amount = dto.Amount
            };
        }

        private OrderDetailDto MapToOrderDetailDto(OrderDetail order)
        {
            return new OrderDetailDto
            {
                OrderId = order.OrderId,
                ItemId = order.ItemId,
                Quantity = order.Quantity,
                Rate = order.Rate,
                Amount = order.Amount
            };
        }

        public async Task<ServiceResponse<OrderDetailDto>> GetOrderDetailsByIdAsync(int orderDetailId, bool includeDetails = false)
        {
            var response = new ServiceResponse<OrderDetailDto>();
            try
            {
                var query = _context.OrderDetails.AsQueryable();
                if (includeDetails)
                {
                    query = query.Include(o => o.Order)
                                 .Include(o => o.Item);
                }

                var order = await query.FirstOrDefaultAsync(o => o.OrderDetailId == orderDetailId);
                if (order == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Order details with ID {orderDetailId} not found";
                    return response;
                }

                response.Data = MapToOrderDetailDto(order);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details with ID {Id}: {Message}", orderDetailId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving order details with ID {orderDetailId}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<IEnumerable<OrderDetailDto>>> GetAllOrderDetailsAsync(int page = 1, int pageSize = 10, bool includeDetails = false)
        {
            var response = new ServiceResponse<IEnumerable<OrderDetailDto>>();
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Invalid page or pageSize";
                    return response;
                }

                var query = _context.OrderDetails.AsQueryable();
                if (includeDetails)
                {
                    query = query.Include(o => o.Order)
                                 .Include(o => o.Item);
                }

                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                response.Data = orders.Select(MapToOrderDetailDto).ToList();
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving order details: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<OrderDetailDto>> CreateOrderDetailsAsync(OrderDetailDto orderDto)
        {
            var response = new ServiceResponse<OrderDetailDto>();
            try
            {
                if (!ValidateOrderDetailDto(orderDto, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                var order = MapToOrderDetail(orderDto);

                await using var transaction = await _context.Database.BeginTransactionAsync();
                _context.OrderDetails.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var createdOrder = await _context.OrderDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderDetailId == order.OrderDetailId);

                if (createdOrder == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Failed to retrieve the created order details";
                    return response;
                }

                response.Data = MapToOrderDetailDto(createdOrder);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating order details: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error creating order details: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<OrderDetailDto>> UpdateOrderDetailsAsync(OrderDetailDto orderDto)
        {
            var response = new ServiceResponse<OrderDetailDto>();
            try
            {
                if (!ValidateOrderDetailDto(orderDto, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                var existingOrder = await _context.OrderDetails.FindAsync(orderDto.OrderDetailId);
                if (existingOrder == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Order detail with ID {orderDto.OrderDetailId} not found";
                    return response;
                }

                existingOrder.ItemId = orderDto.ItemId;
                existingOrder.OrderId = orderDto.OrderId;
                existingOrder.Quantity = orderDto.Quantity;


                await using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = MapToOrderDetailDto(existingOrder);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating order detail with ID {Id}: {Message}", orderDto.OrderDetailId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Concurrency error updating order details with ID {orderDto.OrderDetailId}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating order details with ID {Id}: {Message}", orderDto.OrderDetailId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error updating order Details with ID {orderDto.OrderDetailId}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> DeleteOrderDetailsAsync(int orderDetailId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var order = await _context.OrderDetails.FindAsync(orderDetailId);
                if (order == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Order details with ID {orderDetailId} not found";
                    response.Data = false;
                    return response;
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();
                _context.OrderDetails.Remove(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = true;
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting order details with ID {Id}: {Message}", orderDetailId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error deleting order details with ID {orderDetailId}: {ex.Message}";
                response.Data = false;
                return response;
            }
        }


    }
}