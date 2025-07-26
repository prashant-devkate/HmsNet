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
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;
        private const int MaxStatusLength = 50;

        public OrderService(AppDbContext context, ILogger<OrderService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool ValidateOrderDto(OrderDto order, out string errorMessage)
        {
            if (order == null)
            {
                errorMessage = "Order cannot be null";
                return false;
            }
            if (order.RoomId <= 0)
            {
                errorMessage = "Room ID must be greater than zero";
                return false;
            }
            if (string.IsNullOrWhiteSpace(order.Status))
            {
                errorMessage = "Order status is required";
                return false;
            }
            if (order.Status.Length > MaxStatusLength)
            {
                errorMessage = $"Status cannot exceed {MaxStatusLength} characters";
                return false;
            }
            errorMessage = null;
            return true;
        }

        private Order MapToOrder(OrderDto dto)
        {
            return new Order
            {
                OrderId = dto.OrderId,
                RoomId = dto.RoomId,
                OrderDateTime = dto.OrderDateTime,
                Status = dto.Status?.Trim(),
                TotalAmount = dto.TotalAmount
            };
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                RoomId = order.RoomId,
                OrderDateTime = order.OrderDateTime,
                Status = order.Status,
                TotalAmount = order.TotalAmount
            };
        }

        public async Task<ServiceResponse<OrderDto>> GetOrderByIdAsync(int orderId, bool includeDetails = false)
        {
            var response = new ServiceResponse<OrderDto>();
            try
            {
                var query = _context.Orders.AsQueryable();
                if (includeDetails)
                {
                    query = query.Include(o => o.OrderDetails)
                                 .Include(o => o.Bill)
                                 .Include(o => o.Room);
                }

                var order = await query.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Order with ID {orderId} not found";
                    return response;
                }

                response.Data = MapToOrderDto(order);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with ID {Id}: {Message}", orderId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving order with ID {orderId}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<IEnumerable<OrderDto>>> GetAllOrdersAsync(int page = 1, int pageSize = 10, bool includeDetails = false)
        {
            var response = new ServiceResponse<IEnumerable<OrderDto>>();
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Invalid page or pageSize";
                    return response;
                }

                var query = _context.Orders.AsQueryable();
                if (includeDetails)
                {
                    query = query.Include(o => o.OrderDetails)
                                 .Include(o => o.Bill)
                                 .Include(o => o.Room);
                }

                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                response.Data = orders.Select(MapToOrderDto).ToList();
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving orders: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<OrderDto>> CreateOrderAsync(OrderDto orderDto)
        {
            var response = new ServiceResponse<OrderDto>();
            try
            {
                if (!ValidateOrderDto(orderDto, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                var order = MapToOrder(orderDto);
                order.OrderDateTime = DateTime.Now;
                order.Status = orderDto.Status ?? "Pending";
                order.TotalAmount = 0; // To be updated later with OrderDetails

                await using var transaction = await _context.Database.BeginTransactionAsync();
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = MapToOrderDto(order);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating order: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error creating order: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<OrderDto>> UpdateOrderAsync(OrderDto orderDto)
        {
            var response = new ServiceResponse<OrderDto>();
            try
            {
                if (!ValidateOrderDto(orderDto, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                var existingOrder = await _context.Orders.FindAsync(orderDto.OrderId);
                if (existingOrder == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Order with ID {orderDto.OrderId} not found";
                    return response;
                }

                existingOrder.RoomId = orderDto.RoomId;
                existingOrder.Status = orderDto.Status?.Trim();
                existingOrder.TotalAmount = orderDto.TotalAmount;

                await using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = MapToOrderDto(existingOrder);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating order with ID {Id}: {Message}", orderDto.OrderId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Concurrency error updating order with ID {orderDto.OrderId}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating order with ID {Id}: {Message}", orderDto.OrderId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error updating order with ID {orderDto.OrderId}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> DeleteOrderAsync(int orderId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Order with ID {orderId} not found";
                    response.Data = false;
                    return response;
                }

                if (await _context.OrderDetails.AnyAsync(od => od.OrderId == orderId))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Cannot delete order with active order details";
                    response.Data = false;
                    return response;
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = true;
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting order with ID {Id}: {Message}", orderId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error deleting order with ID {orderId}: {ex.Message}";
                response.Data = false;
                return response;
            }
        }

        public async Task<ServiceResponse<IEnumerable<OrderDto>>> GetOrdersByRoomIdAsync(int roomId, int page = 1, int pageSize = 10, bool includeDetails = false)
        {
            var response = new ServiceResponse<IEnumerable<OrderDto>>();
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Invalid page or pageSize";
                    return response;
                }

                var query = _context.Orders.Where(o => o.RoomId == roomId).AsQueryable();
                if (includeDetails)
                {
                    query = query.Include(o => o.OrderDetails)
                                 .Include(o => o.Bill)
                                 .Include(o => o.Room);
                }

                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                response.Data = orders.Select(MapToOrderDto).ToList();
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for room ID {RoomId}: {Message}", roomId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving orders for room ID {roomId}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<decimal>> CalculateTotalAmountAsync(int orderId)
        {
            var response = new ServiceResponse<decimal>();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Order with ID {orderId} not found";
                    return response;
                }

                var total = order.OrderDetails?.Sum(od => od.Amount) ?? 0m;
                response.Data = total;
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total amount for order ID {Id}: {Message}", orderId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error calculating total amount for order ID {orderId}: {ex.Message}";
                response.Data = 0m;
                return response;
            }
        }
    }
}