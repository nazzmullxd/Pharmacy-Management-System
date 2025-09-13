using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;
using Database.Model;

namespace Business.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProductBatchRepository _productBatchRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IPurchaseRepository _purchaseRepository;

        public PurchaseOrderService(
            ISupplierRepository supplierRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IProductBatchRepository productBatchRepository,
            IAuditLogRepository auditLogRepository,
            IPurchaseRepository purchaseRepository)
        {
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _productBatchRepository = productBatchRepository ?? throw new ArgumentNullException(nameof(productBatchRepository));
            _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
            _purchaseRepository = purchaseRepository ?? throw new ArgumentNullException(nameof(purchaseRepository));
        }

        public async Task<IEnumerable<PurchaseOrderDTO>> GetAllPurchaseOrdersAsync()
        {
            var orders = await _purchaseRepository.GetAllAsync();
            return orders.Select(MapToDTO);
        }

        public async Task<PurchaseOrderDTO?> GetPurchaseOrderByIdAsync(Guid orderId)
        {
            var order = await _purchaseRepository.GetByIdAsync(orderId);
            return order != null ? MapToDTO(order) : null;
        }

        public async Task<PurchaseOrderDTO> CreatePurchaseOrderAsync(PurchaseOrderDTO orderDto)
        {
            if (orderDto == null)
                throw new ArgumentNullException(nameof(orderDto));

            ValidatePurchaseOrder(orderDto);

            var order = MapToEntity(orderDto);
            order.PurchaseID = Guid.NewGuid();
            order.OrderNumber = await GenerateOrderNumberAsync();
            order.OrderDate = DateTime.UtcNow;
            order.Status = "Pending";
            order.PaymentStatus = "Pending";
            order.DueAmount = order.TotalAmount - order.PaidAmount;

            await _purchaseRepository.AddAsync(order);
            await LogPurchaseOrderCreation(orderDto);

            return MapToDTO(order);
        }

        public async Task<PurchaseOrderDTO> UpdatePurchaseOrderAsync(PurchaseOrderDTO orderDto)
        {
            if (orderDto == null)
                throw new ArgumentNullException(nameof(orderDto));

            ValidatePurchaseOrder(orderDto);

            var order = await _purchaseRepository.GetByIdAsync(orderDto.PurchaseOrderID);
            if (order == null)
                throw new InvalidOperationException("Purchase order not found");

            order.TotalAmount = orderDto.TotalAmount;
            order.PaidAmount = orderDto.PaidAmount;
            order.DueAmount = orderDto.TotalAmount - orderDto.PaidAmount;
            order.Status = orderDto.Status;
            order.PaymentStatus = orderDto.PaymentStatus;

            await _purchaseRepository.UpdateAsync(order);
            await LogPurchaseOrderUpdate(orderDto);

            return MapToDTO(order);
        }

        public async Task<bool> DeletePurchaseOrderAsync(Guid orderId)
        {
            try
            {
                var order = await _purchaseRepository.GetByIdAsync(orderId);
                if (order == null)
                    return false;

                await _purchaseRepository.DeleteAsync(orderId);
                await LogAudit("DELETE", "PurchaseOrder", orderId, "Purchase order deleted");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<PurchaseOrderDTO>> GetOrdersBySupplierAsync(Guid supplierId)
        {
            var orders = await _purchaseRepository.GetBySupplierIdAsync(supplierId);
            return orders.Select(MapToDTO);
        }

        public async Task<IEnumerable<PurchaseOrderDTO>> GetOrdersByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be null or empty", nameof(status));

            var orders = await _purchaseRepository.GetAllAsync();
            return orders.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                         .Select(MapToDTO);
        }

        public async Task<IEnumerable<PurchaseOrderDTO>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _purchaseRepository.GetByDateRangeAsync(startDate, endDate);
            return orders.Select(MapToDTO);
        }

        public async Task<bool> ApprovePurchaseOrderAsync(Guid orderId, Guid approvedBy)
        {
            try
            {
                var order = await _purchaseRepository.GetByIdAsync(orderId);
                if (order == null)
                    return false;

                order.Status = "Approved";
                await _purchaseRepository.UpdateAsync(order);
                await LogAudit("APPROVE", "PurchaseOrder", orderId, $"Purchase order approved by {approvedBy}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ReceivePurchaseOrderAsync(Guid orderId, List<PurchaseOrderItemDTO> receivedItems)
        {
            try
            {
                if (receivedItems == null || !receivedItems.Any())
                    return false;

                var order = await _purchaseRepository.GetByIdAsync(orderId);
                if (order == null)
                    return false;

                foreach (var item in receivedItems)
                {
                    if (item.ReceivedQuantity > 0)
                    {
                        var batch = new ProductBatch
                        {
                            ProductBatchID = Guid.NewGuid(),
                            ProductID = item.ProductID,
                            SupplierID = order.SupplierID,
                            BatchNumber = item.BatchNumber,
                            ExpiryDate = item.ExpiryDate ?? DateTime.UtcNow.AddYears(2),
                            QuantityInStock = item.ReceivedQuantity,
                            CreatedDate = DateTime.UtcNow
                        };

                        await _productBatchRepository.AddAsync(batch);
                    }
                }

                order.Status = "Delivered";
                await _purchaseRepository.UpdateAsync(order);
                await LogAudit("RECEIVE", "PurchaseOrder", orderId, $"Purchase order received with {receivedItems.Count} items");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CancelPurchaseOrderAsync(Guid orderId, string reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reason))
                    return false;

                var order = await _purchaseRepository.GetByIdAsync(orderId);
                if (order == null)
                    return false;

                order.Status = "Cancelled";
                await _purchaseRepository.UpdateAsync(order);
                await LogAudit("CANCEL", "PurchaseOrder", orderId, $"Purchase order cancelled: {reason}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<string> GenerateOrderNumberAsync()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return Task.FromResult($"PO-{date}-{random}");
        }

        public async Task<decimal> GetTotalOrderValueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var orders = await _purchaseRepository.GetByDateRangeAsync(startDate ?? DateTime.MinValue, endDate ?? DateTime.MaxValue);
            return orders.Sum(o => o.TotalAmount);
        }

        public async Task<IEnumerable<PurchaseOrderDTO>> GetPendingOrdersAsync()
        {
            return await GetOrdersByStatusAsync("Pending");
        }

        public async Task<IEnumerable<PurchaseOrderDTO>> GetOverdueOrdersAsync()
        {
            var orders = await _purchaseRepository.GetAllAsync();
            return orders.Where(o => o.DueAmount > 0 && o.OrderDate < DateTime.UtcNow)
                         .Select(MapToDTO);
        }

        private void ValidatePurchaseOrder(PurchaseOrderDTO orderDto)
        {
            if (orderDto.SupplierID == Guid.Empty)
                throw new ArgumentException("Supplier ID is required", nameof(orderDto.SupplierID));

            if (orderDto.CreatedBy == Guid.Empty)
                throw new ArgumentException("Created by user ID is required", nameof(orderDto.CreatedBy));

            if (orderDto.TotalAmount <= 0)
                throw new ArgumentException("Total amount must be greater than zero", nameof(orderDto.TotalAmount));

            if (orderDto.PaidAmount < 0)
                throw new ArgumentException("Paid amount cannot be negative", nameof(orderDto.PaidAmount));

            if (orderDto.PaidAmount > orderDto.TotalAmount)
                throw new ArgumentException("Paid amount cannot exceed total amount", nameof(orderDto.PaidAmount));

            if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
                throw new ArgumentException("Purchase order must have at least one item", nameof(orderDto.OrderItems));
        }

        private async Task LogPurchaseOrderCreation(PurchaseOrderDTO orderDto)
        {
            var details = $"Purchase order created: {orderDto.OrderNumber}, Total: {orderDto.TotalAmount:C}";
            await LogAudit("CREATE", "PurchaseOrder", orderDto.PurchaseOrderID, details);
        }

        private async Task LogPurchaseOrderUpdate(PurchaseOrderDTO orderDto)
        {
            var details = $"Purchase order updated: {orderDto.OrderNumber}, Status: {orderDto.Status}";
            await LogAudit("UPDATE", "PurchaseOrder", orderDto.PurchaseOrderID, details);
        }

        private async Task LogAudit(string action, string entityType, Guid entityId, string details)
        {
            var auditLog = new AuditLog
            {
                AuditLogID = Guid.NewGuid(),
                UserID = Guid.Empty, // Replace with actual user ID
                Action = action,
                ActionDate = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);
            Console.WriteLine($"AUDIT: {action} on {entityType} {entityId}: {details}");
        }

        private PurchaseOrderDTO MapToDTO(Purchase order)
        {
            return new PurchaseOrderDTO
            {
                PurchaseOrderID = order.PurchaseID,
                OrderNumber = order.OrderNumber,
                SupplierID = order.SupplierID,
                OrderDate = order.PurchaseDate,
                TotalAmount = order.TotalAmount,
                PaidAmount = order.PaidAmount,
                DueAmount = order.TotalAmount - order.PaidAmount,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus
            };
        }

        private Purchase MapToEntity(PurchaseOrderDTO orderDto)
        {
            return new Purchase
            {
                PurchaseID = orderDto.PurchaseOrderID,
                SupplierID = orderDto.SupplierID,
                PurchaseDate = orderDto.OrderDate,
                TotalAmount = orderDto.TotalAmount,
                PaidAmount = orderDto.PaidAmount,
                Status = orderDto.Status,
                PaymentStatus = orderDto.PaymentStatus
            };
        }
    }
}