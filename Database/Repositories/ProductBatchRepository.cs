using Database.Context;
using Database.Interfaces;
using Database.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Repositories
{

    public class ProductBatchRepository : IProductBatchRepository
    {
        private readonly PharmacyManagementContext _context;
        public ProductBatchRepository(PharmacyManagementContext Context)
        {
            _context = Context;
        }
        public async Task<IEnumerable<ProductBatch>> GetAllAsync()
        {
            return await _context.ProductBatches.ToListAsync();
        }
        public async Task<ProductBatch?> GetByIdAsync(Guid productBatchId)
        {
            return await _context.ProductBatches.FirstOrDefaultAsync(pb => pb.ProductBatchID == productBatchId);


        }
        public async Task AddAsync(ProductBatch productBatch)
        {
            await _context.ProductBatches.AddAsync(productBatch);
            await _context.SaveChangesAsync();

        }
        public async Task UpdateAsync(ProductBatch productBatch)
        {
            _context.ProductBatches.Update(productBatch);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid productBatchId)
        {
            var productBatch = await _context.ProductBatches.FirstOrDefaultAsync(pb => pb.ProductBatchID == productBatchId);
            if (productBatch != null)
            {
                _context.ProductBatches.Remove(productBatch);
                await _context.SaveChangesAsync();

            }
        }
        public async Task<IEnumerable<ProductBatch>> GetByProductIdAsync(Guid productId)
        {
            return await _context.ProductBatches
                .Where(pb => pb.ProductID == productId)
                .ToListAsync();

        }

    }
}
