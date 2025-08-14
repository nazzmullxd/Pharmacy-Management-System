using Database.Context;
using Database.Model;
using Database.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly PharmacyManagementContext _context;

        public ProductRepository(PharmacyManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid productId)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}