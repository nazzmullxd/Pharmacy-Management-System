using Database.Context;
using Database.Model;
using Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class SaleItemRepository: ISaleItemRepository
    {
        private readonly PharmacyManagementContext _context;
        public SaleItemRepository(PharmacyManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SaleItem>> GetAll()
        { return await _context.SalesItems.ToListAsync(); }
        public async Task<SaleItem?> GetByIdAsync(Guid saleItemId)
        {
            return await _context.SalesItems.FirstOrDefaultAsync(si => si.SaleItemID == saleItemId);
        }
        public async Task AddAsync(SaleItem saleItem)
        {
            if (saleItem == null)
            { 
                throw new ArgumentNullException(nameof(saleItem),"Sale item cannot be null.");
            }
            await _context.SalesItems.AddAsync(saleItem);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(SaleItem saleItem)
        {
            if (saleItem == null)
            {
                throw new ArgumentNullException(nameof(saleItem), "Sale item cannot be null.");
            }
             _context.SalesItems.Update(saleItem);
            await _context.SaveChangesAsync();

        }
        public async Task DeleteAsync(Guid saleItemId)
        {
            var saleItem = await GetByIdAsync(saleItemId);
            if (saleItem == null)
            {
                throw new ArgumentNullException(nameof(saleItemId), $"Sale item with ID ({saleItemId}) cannot be found.");
            }
            _context.SalesItems.Remove(saleItem);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<SaleItem>> GetBySaleIdAsync(Guid saleId)
        {
            return await _context.SalesItems
                .Where(si => si.SaleID == saleId)
                .ToListAsync();
        }
        public async Task<IEnumerable<SaleItem>> GetByProductIdAsync(Guid productId)
        {
            return await _context.SalesItems
                .Where(si => si.ProductID == productId)
                .ToListAsync();
        }
    }
}
