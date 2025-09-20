using Database.Context;
using Database.Model;
using Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class PurchaseRepository: IPurchaseRepository
    {
        private readonly PharmacyManagementContext _context;

        public PurchaseRepository(PharmacyManagementContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Purchase>> GetAllAsync()
        {
            try
            {
                Console.WriteLine("Loading purchases with nullable foreign keys handling...");
                
                // Use a more tolerant approach that doesn't fail on missing foreign keys
                // First get basic purchases without includes to avoid INNER JOIN issues
                var purchases = await _context.Purchases
                    .AsNoTracking()
                    .ToListAsync();
                
                Console.WriteLine($"GetAllAsync: Found {purchases.Count} purchases without includes");
                
                // For each purchase, try to load related data separately
                foreach (var purchase in purchases)
                {
                    // Try to load supplier safely - check for null and empty GUIDs
                    try
                    {
                        if (purchase.SupplierID.HasValue && purchase.SupplierID.Value != Guid.Empty)
                        {
                            purchase.Supplier = await _context.Suppliers
                                .FirstOrDefaultAsync(s => s.SupplierID == purchase.SupplierID.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not load supplier for purchase {purchase.PurchaseID}: {ex.Message}");
                        purchase.Supplier = null;
                    }
                    
                    // Try to load user safely - check for null and empty GUIDs
                    try
                    {
                        if (purchase.UserID.HasValue && purchase.UserID.Value != Guid.Empty)
                        {
                            purchase.User = await _context.UsersInfo
                                .FirstOrDefaultAsync(u => u.UserID == purchase.UserID.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not load user for purchase {purchase.PurchaseID}: {ex.Message}");
                        purchase.User = null;
                    }

                    // Load PurchaseItems for this purchase
                    try
                    {
                        purchase.PurchaseItems = await _context.PurchaseItems
                            .Include(pi => pi.Product)
                            .AsNoTracking()
                            .Where(pi => pi.PurchaseID == purchase.PurchaseID)
                            .ToListAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not load purchase items for purchase {purchase.PurchaseID}: {ex.Message}");
                        purchase.PurchaseItems = new List<PurchaseItem>();
                    }
                }
                
                Console.WriteLine($"Successfully processed {purchases.Count} purchases with related data");
                return purchases;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
                // Fallback to basic query without any includes
                return await GetBasicOrdersAsync();
            }
        }

        public async Task<IEnumerable<Purchase>> GetBasicOrdersAsync()
        {
            return await _context.Purchases
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetPurchaseCountAsync()
        {
            try
            {
                // Raw SQL query to count purchases without any EF mapping issues
                var count = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM Purchases").FirstOrDefaultAsync();
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPurchaseCountAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<Purchase?> GetByIdAsync(Guid purchaseId)
        {
            if (purchaseId == Guid.Empty)
            {
                throw new ArgumentException("Invalid purchase ID", nameof(purchaseId));
            }

            return await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(p => p.PurchaseID == purchaseId);
        }

        public async Task AddAsync(Purchase purchase)
        {
            if (purchase == null)
            {
                throw new ArgumentNullException(nameof(purchase));
            }

            await _context.Purchases.AddAsync(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Purchase purchase)
        {
            if (purchase == null)
            {
                throw new ArgumentNullException(nameof(purchase));
            }

            _context.Purchases.Update(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid purchaseId)
        {
            if (purchaseId == Guid.Empty)
            {
                throw new ArgumentException("Invalid purchase ID", nameof(purchaseId));
            }

            var purchase = await _context.Purchases.FindAsync(purchaseId);
            if (purchase == null)
            {
                throw new KeyNotFoundException("Purchase not found");
            }

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Purchase>> GetBySupplierIdAsync(Guid supplierId)
        {
            if (supplierId == Guid.Empty)
            {
                throw new ArgumentException("Invalid supplier ID", nameof(supplierId));
            }

            return await _context.Purchases
                .Where(p => p.SupplierID == supplierId)
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .ToListAsync();
        }

      /*  public async Task<IEnumerable<Purchase>> GetByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("Invalid user ID", nameof(userId));
            }

            return await _context.Purchases
                .Where(p => p.UserID == userId)
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .Include(p => p.ProductBatch)
                .ToListAsync();
        }*/

        public async Task<IEnumerable<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Purchases
                .Where(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate)
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .ToListAsync();
        }
    }
}