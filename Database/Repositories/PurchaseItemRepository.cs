using Database.Context;
using Database.Model;
using Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class PurchaseItemRepository: IPurchaseItemRepository
    {
        private readonly PharmacyManagementContext _context;
        public PurchaseItemRepository(PharmacyManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PurchaseItem>> GetAllAsync()
        {   return await _context.PurchaseItems.ToListAsync(); }
        public async Task<PurchaseItem?> GetByIdAsync(Guid purchaseItemId)
            {
            return await _context.PurchaseItems.FirstOrDefaultAsync(pi => pi.PurchaseItemID == purchaseItemId);
        }
        public async Task AddAsync(PurchaseItem purchaseItem)
            {
            if (purchaseItem == null)
            {
                throw new ArgumentNullException(nameof(purchaseItem), "Purchase item cannot be null.");
            }
            await _context.PurchaseItems.AddAsync(purchaseItem);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(PurchaseItem purchaseItem)
        {             if (purchaseItem == null)
            {
                throw new ArgumentNullException(nameof(purchaseItem), "Purchase item cannot be null.");
            }
            _context.PurchaseItems.Update(purchaseItem);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid purchaseItemId)
        {
            var purchaseItem = await GetByIdAsync(purchaseItemId);
            if (purchaseItem == null)
            {
                throw new ArgumentNullException(nameof(purchaseItemId), $"Purchase item with ID ({purchaseItemId}) cannot be found.");
            }
            _context.PurchaseItems.Remove(purchaseItem);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<PurchaseItem>> GetByPurchaseIdAsync(Guid purchaseId)
        {             return await _context.PurchaseItems
                .Where(pi => pi.PurchaseID == purchaseId)
                .ToListAsync();
        }
    }
}
