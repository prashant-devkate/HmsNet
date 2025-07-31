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

    public class BillService : IBillService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BillService> _logger;
        private const int MaxNameLength = 100;

        public BillService(AppDbContext context, ILogger<BillService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool ValidateBillDto(BillDto bill, out string errorMessage)
        {
            if (bill == null)
            {
                errorMessage = "Bill cannot be null";
                return false;
            }
            if (bill.OrderId <= 0)
            {
                errorMessage = "OrderId must be greater than zero";
                return false;
            }
            if (bill.TotalAmount <= 0 )
            {
                errorMessage = $"Total amount cannot be zero";
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

        private Bill MapToBill(BillDto dto)
        {
            return new Bill
            {
                BillId = dto.BillId,
                OrderId = dto.OrderId,
                BillDateTime = dto.BillDateTime,
                TotalAmount = dto.TotalAmount,
                DiscountAmount = dto.DiscountAmount,
                TaxAmount = dto.TaxAmount,
                FinalAmount = dto.FinalAmount,
                PaymentStatus = dto.PaymentStatus
            };
        }

        private BillDto MapToBillDto(Bill bill)
        {
            return new BillDto
            {
                BillId = bill.BillId,
                OrderId = bill.OrderId,
                BillDateTime = bill.BillDateTime,
                TotalAmount = bill.TotalAmount,
                DiscountAmount = bill.DiscountAmount,
                TaxAmount = bill.TaxAmount,
                FinalAmount = bill.FinalAmount,
                PaymentStatus = bill.PaymentStatus
            };
        }

        public async Task<ServiceResponse<IEnumerable<BillDto>>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var response = new ServiceResponse<IEnumerable<BillDto>>();
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = "Invalid page or pageSize";
                    return response;
                }

                var query = _context.Bills.AsQueryable();


                var bill = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new BillDto
                    {
                        BillId = r.BillId,
                        OrderId = r.OrderId,
                        BillDateTime = r.BillDateTime,
                        TotalAmount = r.TotalAmount,
                        DiscountAmount = r.DiscountAmount,
                        TaxAmount = r.TaxAmount,
                        FinalAmount = r.FinalAmount,
                        PaymentStatus = r.PaymentStatus
                    }).ToListAsync();

                response.Data = bill;
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bills: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving bills: {ex.Message}";
                response.Data = null;
                return response;
            }
        }
        public async Task<ServiceResponse<BillDto>> GetByIdAsync(int id)
        {
            var response = new ServiceResponse<BillDto>();
            try
            {
                var query = _context.Bills.AsQueryable();
               
                var bill = await query.FirstOrDefaultAsync(i => i.BillId == id);
                if (bill == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Bill with ID {id} not found";
                    return response;
                }

                response.Data = MapToBillDto(bill);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bill with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error retrieving bill with ID {id}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<BillDto>> CreateAsync(BillDto billDto)
        {
            var response = new ServiceResponse<BillDto>();
            try
            {
                if (!ValidateBillDto(billDto, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                var duplicateBillCheck = await _context.Bills.AnyAsync(r => r.OrderId == billDto.OrderId);

                if (duplicateBillCheck)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Bill for this order already exists.";
                    return response;
                }

                var bill = MapToBill(billDto);
                bill.PaymentStatus = "Pending"; // Ensure new bills are pending

                await using var transaction = await _context.Database.BeginTransactionAsync();
                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = MapToBillDto(bill);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating bill: {Message}", ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error creating bill: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<BillDto>> UpdateAsync(BillDto billDto)
        {
            var response = new ServiceResponse<BillDto>();
            try
            {
                if (!ValidateBillDto(billDto, out var errorMessage))
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = errorMessage;
                    return response;
                }

                var existingBill = await _context.Bills.FindAsync(billDto.BillId);
                if (existingBill == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Bill with ID {billDto.BillId} not found";
                    return response;
                }

                existingBill.OrderId = billDto.OrderId;
                existingBill.BillDateTime = billDto.BillDateTime;
                existingBill.TotalAmount = billDto.TotalAmount;
                existingBill.FinalAmount = billDto.FinalAmount;
                existingBill.DiscountAmount = billDto.DiscountAmount;
                existingBill.TaxAmount = billDto.TaxAmount;
                existingBill.PaymentStatus = billDto.PaymentStatus;

                await using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = MapToBillDto(existingBill);
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating bill with ID {Id}: {Message}", billDto.BillId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Concurrency error updating bill with ID {billDto.BillId}: {ex.Message}";
                response.Data = null;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating bill with ID {Id}: {Message}", billDto.BillId, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error updating bill with ID {billDto.BillId}: {ex.Message}";
                response.Data = null;
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var bill = await _context.Bills.FindAsync(id);
                if (bill == null)
                {
                    response.Status = ResponseStatus.Error;
                    response.Message = $"Bill with ID {id} not found";
                    response.Data = false;
                    return response;
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Bills.Remove(bill);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                response.Data = true;
                response.Status = ResponseStatus.Success;
                return response;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting bill with ID {Id}: {Message}", id, ex.Message);
                response.Status = ResponseStatus.Error;
                response.Message = $"Error deleting bill with ID {id}: {ex.Message}";
                response.Data = false;
                return response;
            }
        }
    }
}