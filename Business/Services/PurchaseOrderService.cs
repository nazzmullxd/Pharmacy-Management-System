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
            try
            {
                Console.WriteLine("Starting GetAllPurchaseOrdersAsync");
                
                // Use raw SQL query to bypass EF mapping issues
                var rawCount = await _purchaseRepository.GetPurchaseCountAsync();
                Console.WriteLine($"Raw purchase count from database: {rawCount}");
                
                if (rawCount == 0)
                {
                    Console.WriteLine("No purchases found - returning empty list");
                    return Enumerable.Empty<PurchaseOrderDTO>();
                }
                
                // Try to get purchases with the improved repository method
                var orders = await _purchaseRepository.GetAllAsync();
                Console.WriteLine($"Purchase count from repo: {orders.Count()}");
                
                if (orders.Any())
                {
                    Console.WriteLine($"Successfully loaded {orders.Count()} orders");
                    return orders.Select(MapToDTO);
                }
                else
                {
                    Console.WriteLine("Repository returned empty despite raw count showing data");
                    return Enumerable.Empty<PurchaseOrderDTO>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllPurchaseOrdersAsync: {ex.Message}");
                Console.WriteLine("Returning empty list due to error");
                return Enumerable.Empty<PurchaseOrderDTO>();
            }
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
            order.PurchaseDate = DateTime.UtcNow; // Set both dates properly
            order.CreatedDate = DateTime.UtcNow;
            order.Status = "Pending";
            order.PaymentStatus = "Pending";
            order.DueAmount = order.TotalAmount - order.PaidAmount;

            // Set UserID - using a default user ID for now since authentication isn't implemented
            // TODO: Replace with actual authenticated user ID
            order.UserID = orderDto.CreatedBy != Guid.Empty ? orderDto.CreatedBy : await GetDefaultUserIdAsync();

            // ProductBatchID is not needed at order creation level - it's for individual items
            order.ProductBatchID = Guid.Empty;

            // Map order items to purchase items
            if (orderDto.OrderItems != null && orderDto.OrderItems.Any())
            {
                var purchaseItems = new List<PurchaseItem>();
                foreach (var item in orderDto.OrderItems)
                {
                    // Get a valid ProductBatch for this product
                    var productBatches = await _productBatchRepository.GetByProductIdAsync(item.ProductID);
                    var productBatch = productBatches?.FirstOrDefault();
                    
                    // If no batch exists for this product, get any available batch as fallback
                    if (productBatch == null)
                    {
                        var allBatches = await _productBatchRepository.GetAllAsync();
                        productBatch = allBatches?.FirstOrDefault();
                    }

                    purchaseItems.Add(new PurchaseItem
                    {
                        PurchaseItemID = Guid.NewGuid(),
                        PurchaseID = order.PurchaseID,
                        ProductID = item.ProductID,
                        Quantity = item.OrderedQuantity,
                        UnitPrice = item.UnitPrice,
                        ProductBatchID = productBatch?.ProductBatchID ?? Guid.NewGuid(), // Use existing batch or generate new ID
                        CreatedDate = DateTime.UtcNow
                    });
                }
                order.PurchaseItems = purchaseItems;
            }

            await _purchaseRepository.AddAsync(order);
         //   await LogPurchaseOrderCreation(orderDto);

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
         //   await LogPurchaseOrderUpdate(orderDto);

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
              //  await LogAudit("DELETE", "PurchaseOrder", orderId, "Purchase order deleted");
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
               // await LogAudit("APPROVE", "PurchaseOrder", orderId, $"Purchase order approved by {approvedBy}");
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
                            SupplierID = order.SupplierID ?? Guid.Empty,
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
               // await LogAudit("RECEIVE", "PurchaseOrder", orderId, $"Purchase order received with {receivedItems.Count} items");
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
            //    await LogAudit("CANCEL", "PurchaseOrder", orderId, $"Purchase order cancelled: {reason}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ProcessPurchaseOrderAsync(Guid orderId, Guid processedBy)
        {
            try
            {
                var order = await _purchaseRepository.GetByIdAsync(orderId);
                if (order == null)
                    return false;

                // Change status from Pending to Processed
                if (order.Status?.ToLower() != "pending")
                    return false; // Can only process pending orders

                order.Status = "Processed";
                await _purchaseRepository.UpdateAsync(order);

                // Update inventory for each item in the order
                if (order.PurchaseItems != null && order.PurchaseItems.Any())
                {
                    foreach (var item in order.PurchaseItems)
                    {
                        // Create or update product batch for inventory tracking
                        var existingBatches = await _productBatchRepository.GetByProductIdAsync(item.ProductID);
                        var existingBatch = existingBatches?.FirstOrDefault(b => b.SupplierID == order.SupplierID);

                        if (existingBatch != null)
                        {
                            // Update existing batch
                            existingBatch.QuantityInStock += item.Quantity;
                            await _productBatchRepository.UpdateAsync(existingBatch);
                        }
                        else
                        {
                            // Create new batch
                            var newBatch = new ProductBatch
                            {
                                ProductBatchID = Guid.NewGuid(),
                                ProductID = item.ProductID,
                                SupplierID = order.SupplierID ?? Guid.Empty,
                                BatchNumber = $"BATCH-{DateTime.UtcNow:yyyyMMdd}-{item.ProductID.ToString().Substring(0, 8)}",
                                ExpiryDate = DateTime.UtcNow.AddYears(2), // Default 2 years
                                QuantityInStock = item.Quantity,
                                CreatedDate = DateTime.UtcNow
                            };
                            await _productBatchRepository.AddAsync(newBatch);
                        }
                    }
                }

            //    await LogAudit("PROCESS", "PurchaseOrder", orderId, $"Purchase order processed by {processedBy} - inventory updated");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing purchase order {orderId}: {ex.Message}");
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

           // if (orderDto.CreatedBy == Guid.Empty)
             //   throw new ArgumentException("Created by user ID is required", nameof(orderDto.CreatedBy));

            if (orderDto.TotalAmount <= 0)
                throw new ArgumentException("Total amount must be greater than zero", nameof(orderDto.TotalAmount));

            if (orderDto.PaidAmount < 0)
                throw new ArgumentException("Paid amount cannot be negative", nameof(orderDto.PaidAmount));

            if (orderDto.PaidAmount > orderDto.TotalAmount)
                throw new ArgumentException("Paid amount cannot exceed total amount", nameof(orderDto.PaidAmount));

            if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
                throw new ArgumentException("Purchase order must have at least one item", nameof(orderDto.OrderItems));
        }

        /* 
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
        */
        private PurchaseOrderDTO MapToDTO(Purchase order)
        {
            return new PurchaseOrderDTO
            {
                PurchaseOrderID = order.PurchaseID,
                OrderNumber = order.OrderNumber,
                SupplierID = order.SupplierID ?? Guid.Empty,
                SupplierName = order.Supplier != null ? order.Supplier.SupplierName : "Unknown Supplier",
                CreatedBy = order.UserID ?? Guid.Empty, // Map the UserID to CreatedBy
                CreatedByName = order.User != null ? $"{order.User.FirstName} {order.User.LastName}" : "Unknown User",
                // Prefer OrderDate if available; fall back to PurchaseDate
                OrderDate = order.OrderDate != default ? order.OrderDate : order.PurchaseDate,
                TotalAmount = order.TotalAmount,
                PaidAmount = order.PaidAmount,
                DueAmount = order.TotalAmount - order.PaidAmount,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                Notes = order.Notes,
                OrderItems = order.PurchaseItems?.Select(item => new PurchaseOrderItemDTO
                {
                    PurchaseOrderItemID = item.PurchaseItemID,
                    PurchaseOrderID = item.PurchaseID,
                    ProductID = item.ProductID,
                    ProductName = item.Product?.ProductName ?? "Unknown Product",
                    OrderedQuantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                }).ToList() ?? new List<PurchaseOrderItemDTO>()
            };
        }

        private Purchase MapToEntity(PurchaseOrderDTO orderDto)
        {
            return new Purchase
            {
                PurchaseID = orderDto.PurchaseOrderID,
                SupplierID = orderDto.SupplierID,
                PurchaseDate = orderDto.OrderDate,
                OrderDate = orderDto.OrderDate,
                TotalAmount = orderDto.TotalAmount,
                PaidAmount = orderDto.PaidAmount,
                Status = orderDto.Status,
                PaymentStatus = orderDto.PaymentStatus,
                Notes = orderDto.Notes,
                CreatedDate = DateTime.UtcNow
            };
        }

        private async Task<Guid> GetDefaultUserIdAsync()
        {
            // Try to get the first available user from the database
            try
            {
                var users = await _userRepository.GetAllAsync();
                var firstUser = users.FirstOrDefault();
                if (firstUser != null)
                {
                    return firstUser.UserID;
                }
                
                // If no users exist, return a known default GUID
                // In a real application, you would ensure at least one admin user exists
                return Guid.Parse("00000000-0000-0000-0000-000000000001");
            }
            catch
            {
                // Fallback to a default GUID if database access fails
                return Guid.Parse("00000000-0000-0000-0000-000000000001");
            }
        }
    }
}