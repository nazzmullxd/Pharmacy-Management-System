using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;

namespace Business.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync();
        Task<CustomerDTO?> GetCustomerByIdAsync(Guid customerId);
        Task<CustomerDTO> CreateCustomerAsync(CustomerDTO customerDto);
        Task<CustomerDTO> UpdateCustomerAsync(CustomerDTO customerDto);
        Task<bool> DeleteCustomerAsync(Guid customerId);
        Task<IEnumerable<CustomerDTO>> SearchCustomersByNameAsync(string name);
        Task<IEnumerable<CustomerDTO>> GetCustomersByContactNumberAsync(string contactNumber);
        Task<IEnumerable<CustomerDTO>> GetActiveCustomersAsync();
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeCustomerId = null);
        Task<bool> IsContactNumberUniqueAsync(string contactNumber, Guid? excludeCustomerId = null);
    }
}
