using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;

namespace Business.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDTO>> GetAllSuppliersAsync();
        Task<SupplierDTO?> GetSupplierByIdAsync(Guid supplierId);
        Task<SupplierDTO> CreateSupplierAsync(SupplierDTO supplierDto);
        Task<SupplierDTO> UpdateSupplierAsync(SupplierDTO supplierDto);
        Task<bool> DeleteSupplierAsync(Guid supplierId);
        Task<IEnumerable<SupplierDTO>> GetActiveSuppliersAsync();
        Task<IEnumerable<SupplierDTO>> SearchSuppliersByNameAsync(string supplierName);
        Task<bool> ToggleSupplierStatusAsync(Guid supplierId);
        Task<bool> IsSupplierNameUniqueAsync(string supplierName, Guid? excludeSupplierId = null);
        Task<IEnumerable<SupplierDTO>> GetSuppliersByContactPersonAsync(string contactPerson);
    }
}
