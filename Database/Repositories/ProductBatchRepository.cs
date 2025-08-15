using Database.Context;
using Database.Interfaces;
using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class ProductBatchRepository : IProductBatchRepository
    {
        private readonly PharmacyManagementContext _context;

        public ProductBatchRepository(PharmacyManagementContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null");
        }

        public async Task<IEnumerable<ProductBatch>> GetAllAsync()
        {
            return await _context.ProductBatches.ToListAsync();
        }

        public async Task<ProductBatch?> GetByIdAsync(Guid productBatchId)
        {
            return await _context.ProductBatches
                .FirstOrDefaultAsync(pb => pb.ProductBatchID == productBatchId);
        }

        public async Task AddAsync(ProductBatch productBatch)
        {
            if (productBatch == null)
            {
                throw new ArgumentNullException(nameof(productBatch), "ProductBatch cannot be null");
            }

            await _context.ProductBatches.AddAsync(productBatch);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductBatch productBatch)
        {
            if (productBatch == null)
            {
                throw new ArgumentNullException(nameof(productBatch), "ProductBatch cannot be null");
            }

            var existingProductBatch = await _context.ProductBatches
                .FirstOrDefaultAsync(pb => pb.ProductBatchID == productBatch.ProductBatchID);

            if (existingProductBatch == null)
            {
                throw new InvalidOperationException("ProductBatch not found");
            }

            _context.Entry(existingProductBatch).CurrentValues.SetValues(productBatch);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid productBatchId)
        {
            var productBatch = await _context.ProductBatches
                .FirstOrDefaultAsync(pb => pb.ProductBatchID == productBatchId);

            if (productBatch == null)
            {
                throw new InvalidOperationException("ProductBatch not found");
            }

            _context.ProductBatches.Remove(productBatch);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductBatch>> GetByProductIdAsync(Guid productId)
        {
            return await _context.ProductBatches
                .Where(pb => pb.ProductID == productId)
                .ToListAsync();
        }
    }
}