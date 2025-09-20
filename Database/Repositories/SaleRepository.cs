using Database.Context;
using Database.Interfaces;
using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly PharmacyManagementContext _context;

        public SaleRepository(PharmacyManagementContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null");
        }

        public async Task<IEnumerable<Sale>> GetAllAsync()
        {
            return await _context.Sales
                .ToListAsync();
        }

        public async Task<Sale?> GetByIdAsync(Guid saleId)
        {
            return await _context.Sales
                .FirstOrDefaultAsync(s => s.SaleID == saleId);
        }

        public async Task AddAsync(Sale sale)
        {
            if (sale == null)
            {
                throw new ArgumentNullException(nameof(sale), "Sale cannot be null");
            }

            await _context.Sales.AddAsync(sale);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Sale sale)
        {
            if (sale == null)
            {
                throw new ArgumentNullException(nameof(sale), "Sale cannot be null");
            }

            var existingSale = await _context.Sales.FirstOrDefaultAsync(s => s.SaleID == sale.SaleID);
            if (existingSale == null)
            {
                throw new InvalidOperationException("Sale not found");
            }

            _context.Entry(existingSale).CurrentValues.SetValues(sale);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid saleId)
        {
            var sale = await _context.Sales.FirstOrDefaultAsync(s => s.SaleID == saleId);
            if (sale != null)
            {
                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Sale>> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Sales
                .Where(s => s.CustomerID.HasValue && s.CustomerID.Value == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sale>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Sales
                .Where(s => s.UserID == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Sales
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                .ToListAsync();
        }
    }
}