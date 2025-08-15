using Database.Context;
using Database.Interfaces;
using Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly PharmacyManagementContext _context;

        public CustomerRepository(PharmacyManagementContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Context cannot be null");
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.AsNoTracking().ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(Guid customerId)
        {
            if (customerId == Guid.Empty)
            {
                throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));
            }

            return await _context.Customers.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerID == customerId);
        }

        public async Task AddAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer), "Customer cannot be null");
            }

            await Task.Yield(); // Ensures the method is truly asynchronous
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer), "Customer cannot be null");
            }

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid customerId)
        {
            if (customerId == Guid.Empty)
            {
                throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));
            }

            var customer = await GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new KeyNotFoundException("Customer not found");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
}