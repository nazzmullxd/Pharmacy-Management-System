using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;
using Database.Model;

namespace Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Select(MapToDTO);
        }

        public async Task<CustomerDTO?> GetCustomerByIdAsync(Guid customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            return customer != null ? MapToDTO(customer) : null;
        }

        public async Task<CustomerDTO> CreateCustomerAsync(CustomerDTO customerDto)
        {
            if (customerDto == null)
                throw new ArgumentNullException(nameof(customerDto));

            if (string.IsNullOrWhiteSpace(customerDto.CustomerName))
                throw new ArgumentException("Customer name is required", nameof(customerDto));

            if (string.IsNullOrWhiteSpace(customerDto.ContactNumber))
                throw new ArgumentException("Contact number is required", nameof(customerDto));

            // Check if contact number already exists
            if (!await IsContactNumberUniqueAsync(customerDto.ContactNumber))
                throw new InvalidOperationException("Customer with this contact number already exists");

            var customer = MapToEntity(customerDto);
            customer.CreatedDate = DateTime.UtcNow;

            await _customerRepository.AddAsync(customer);
            return MapToDTO(customer);
        }

        public async Task<CustomerDTO> UpdateCustomerAsync(CustomerDTO customerDto)
        {
            if (customerDto == null)
                throw new ArgumentNullException(nameof(customerDto));

            var existingCustomer = await _customerRepository.GetByIdAsync(customerDto.CustomerID);
            if (existingCustomer == null)
                throw new InvalidOperationException("Customer not found");

            // Update properties
            existingCustomer.CustomerName = customerDto.CustomerName;
            existingCustomer.ContactNumber = customerDto.ContactNumber;
            existingCustomer.Email = customerDto.Email;
            existingCustomer.Address = customerDto.Address;

            await _customerRepository.UpdateAsync(existingCustomer);
            return MapToDTO(existingCustomer);
        }

        public async Task<bool> DeleteCustomerAsync(Guid customerId)
        {
            if (customerId == Guid.Empty)
                throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                return false;

            await _customerRepository.DeleteAsync(customerId);
            return true;
        }

        public async Task<IEnumerable<CustomerDTO>> SearchCustomersByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Search name cannot be null or empty", nameof(name));

            var customers = await _customerRepository.GetAllAsync();
            return customers.Where(c => c.CustomerName.Contains(name, StringComparison.OrdinalIgnoreCase)).Select(MapToDTO);
        }

        public async Task<IEnumerable<CustomerDTO>> GetCustomersByContactNumberAsync(string contactNumber)
        {
            if (string.IsNullOrWhiteSpace(contactNumber))
                throw new ArgumentException("Contact number cannot be null or empty", nameof(contactNumber));

            var customers = await _customerRepository.GetAllAsync();
            return customers.Where(c => c.ContactNumber.Contains(contactNumber, StringComparison.OrdinalIgnoreCase)).Select(MapToDTO);
        }

        public async Task<IEnumerable<CustomerDTO>> GetActiveCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            // For now, all customers are considered active
            // In a real implementation, you might have an IsActive field
            return customers.Select(MapToDTO);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeCustomerId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            var customers = await _customerRepository.GetAllAsync();
            return !customers.Any(c => c.Email == email && c.CustomerID != excludeCustomerId);
        }

        public async Task<bool> IsContactNumberUniqueAsync(string contactNumber, Guid? excludeCustomerId = null)
        {
            if (string.IsNullOrWhiteSpace(contactNumber))
                return true;

            var customers = await _customerRepository.GetAllAsync();
            return !customers.Any(c => c.ContactNumber == contactNumber && c.CustomerID != excludeCustomerId);
        }

        private CustomerDTO MapToDTO(Customer customer)
        {
            return new CustomerDTO
            {
                CustomerID = customer.CustomerID,
                CustomerName = customer.CustomerName,
                ContactNumber = customer.ContactNumber,
                Email = customer.Email,
                Address = customer.Address,
                CreatedDate = customer.CreatedDate,
                LastPurchaseDate = null, // This would be calculated from sales data
                TotalPurchases = 0 // This would be calculated from sales data
            };
        }

        private Customer MapToEntity(CustomerDTO customerDto)
        {
            return new Customer
            {
                CustomerID = customerDto.CustomerID,
                CustomerName = customerDto.CustomerName,
                ContactNumber = customerDto.ContactNumber,
                Email = customerDto.Email,
                Address = customerDto.Address,
                CreatedDate = customerDto.CreatedDate
            };
        }
    }
}
