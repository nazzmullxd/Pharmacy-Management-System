using Database.Model;
using Database.Interfaces;
using Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly PharmacyManagementContext _context;

        public SupplierRepository(PharmacyManagementContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null");
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers.ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(Guid supplierId)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierID == supplierId);
        }

        public async Task AddAsync(Supplier supplier)
        {
            if (supplier == null)
            {
                throw new ArgumentNullException(nameof(supplier), "Supplier cannot be null");
            }

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            if (supplier == null)
            {
                throw new ArgumentNullException(nameof(supplier), "Supplier cannot be null");
            }

            var existingSupplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierID == supplier.SupplierID);

            if (existingSupplier == null)
            {
                throw new InvalidOperationException("Supplier not found");
            }

            _context.Entry(existingSupplier).CurrentValues.SetValues(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid supplierId)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierID == supplierId);

            if (supplier == null)
            {
                throw new InvalidOperationException("Supplier not found");
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Supplier>> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }

            return await _context.Suppliers
                .Where(s => EF.Functions.Like(s.SupplierName, $"%{name}%"))
                .ToListAsync();
        }
    }
}