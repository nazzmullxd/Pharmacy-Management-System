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
            return await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.User)
                .Include(p => p.ProductBatch)
                .ToListAsync();
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
                .Include(p => p.ProductBatch)
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
                .Include(p => p.ProductBatch)
                .ToListAsync();
        }

        public async Task<IEnumerable<Purchase>> GetByUserIdAsync(Guid userId)
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
        }
    }
}