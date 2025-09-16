//This service class manages supplier entities, providing methods to create, read, update, and delete suppliers.
// It includes validation logic to ensure data integrity and methods for searching and filtering suppliers.
// It interacts with a repository interface to perform database operations.


using Business.DTO;
using Business.Interfaces;
using Database.Interfaces;
using Database.Model;

namespace Business.Services
{ 
    //Supplier Service Class
    //Implements ISupplierService Interface
    //Dependency Injection of ISupplierRepository
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
        }

        public async Task<IEnumerable<SupplierDTO>> GetAllSuppliersAsync()
        {
            try
            {
                var suppliers = await _supplierRepository.GetAllAsync();
                return suppliers.Select(MapToDTO);
            }
            catch (Exception)
            {
                // Return empty list if database is not available (for development/demo)
                return Enumerable.Empty<SupplierDTO>();
            }
        }

        public async Task<SupplierDTO?> GetSupplierByIdAsync(Guid supplierId)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierId);
            return supplier != null ? MapToDTO(supplier) : null;
        }
        //Create Supplier Method
        public async Task<SupplierDTO> CreateSupplierAsync(SupplierDTO supplierDto)
        {
            if (supplierDto == null)
                throw new ArgumentNullException(nameof(supplierDto));

            ValidateSupplier(supplierDto);

            var supplier = MapToEntity(supplierDto);
            supplier.SupplierID = Guid.NewGuid();
            supplier.CreatedDate = DateTime.UtcNow;
            supplier.UpdatedDate = DateTime.UtcNow;

            await _supplierRepository.AddAsync(supplier);
            return MapToDTO(supplier);
        }
        //Update Supplier Method
        public async Task<SupplierDTO> UpdateSupplierAsync(SupplierDTO supplierDto)
        {
            if (supplierDto == null)
                throw new ArgumentNullException(nameof(supplierDto));

            var existingSupplier = await _supplierRepository.GetByIdAsync(supplierDto.SupplierID);
            if (existingSupplier == null)
                throw new ArgumentException("Supplier not found", nameof(supplierDto.SupplierID));

            ValidateSupplier(supplierDto);

            var supplier = MapToEntity(supplierDto);
            supplier.CreatedDate = existingSupplier.CreatedDate; // Preserve original creation date
            supplier.UpdatedDate = DateTime.UtcNow;

            await _supplierRepository.UpdateAsync(supplier);
            return MapToDTO(supplier);
        }
        //Delete by SupplierID Method
        //Can be modified to Soft Delete by adding IsActive Flag
        //Here we are doing a hard delete
        public async Task<bool> DeleteSupplierAsync(Guid supplierId)
        {
            try
            {
                await _supplierRepository.DeleteAsync(supplierId);
                return true;
            }
            catch
            {
                return false;
            }
        }
        //Get All Suppliers Method
        public async Task<IEnumerable<SupplierDTO>> GetActiveSuppliersAsync()
        {
            var suppliers = await _supplierRepository.GetAllAsync();
            return suppliers.Where(s => s.IsActive).Select(MapToDTO);
        }
        //Search Suppliers by Name Method
        public async Task<IEnumerable<SupplierDTO>> SearchSuppliersByNameAsync(string supplierName)
        {
            if (string.IsNullOrWhiteSpace(supplierName))
                throw new ArgumentException("Supplier name cannot be null or empty", nameof(supplierName));

            var suppliers = await _supplierRepository.GetAllAsync();
            return suppliers.Where(s => s.SupplierName.Contains(supplierName, StringComparison.OrdinalIgnoreCase))
                          .Select(MapToDTO);
        }
        //Supplier Status Toggle Method
        //Checks whether a Supplier is Active or Not
        public async Task<bool> ToggleSupplierStatusAsync(Guid supplierId)
        {
            try
            {
                var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                if (supplier == null)
                    return false;

                supplier.IsActive = !supplier.IsActive;
                supplier.UpdatedDate = DateTime.UtcNow;
                await _supplierRepository.UpdateAsync(supplier);
                return true;
            }
            catch
            {
                return false;
            }
        }
        //Unique Supplier Name Check Method -- Can be Skipped as there can be multiple suppliers with same name
        //Instead we can use SupplierID as unique identifier
        // However, if business logic requires unique names, this method can be utilized
        public async Task<bool> IsSupplierNameUniqueAsync(string supplierName, Guid? excludeSupplierId = null)
        {
            if (string.IsNullOrWhiteSpace(supplierName))
                return false;

            var suppliers = await _supplierRepository.GetAllAsync();
            return !suppliers.Any(s => s.SupplierName.Equals(supplierName, StringComparison.OrdinalIgnoreCase)
                                     && s.SupplierID != excludeSupplierId);
        }

        public async Task<IEnumerable<SupplierDTO>> GetSuppliersByContactPersonAsync(string contactPerson)
        {
            if (string.IsNullOrWhiteSpace(contactPerson))
                throw new ArgumentException("Contact person cannot be null or empty", nameof(contactPerson));

            var suppliers = await _supplierRepository.GetAllAsync();
            return suppliers.Where(s => s.ContactPerson.Contains(contactPerson, StringComparison.OrdinalIgnoreCase))
                          .Select(MapToDTO);
        }
        //Supplier Validation Method -- Should be skipped if the data is not saved to database
        private void ValidateSupplier(SupplierDTO supplierDto)
        {
            if (string.IsNullOrWhiteSpace(supplierDto.SupplierName))
                throw new ArgumentException("Supplier name is required", nameof(supplierDto.SupplierName));

            if (string.IsNullOrWhiteSpace(supplierDto.ContactPerson))
                throw new ArgumentException("Contact person is required", nameof(supplierDto.ContactPerson));

            if (string.IsNullOrWhiteSpace(supplierDto.PhoneNumber))
                throw new ArgumentException("Phone number is required", nameof(supplierDto.PhoneNumber));

            if (!string.IsNullOrWhiteSpace(supplierDto.Email) && !IsValidEmail(supplierDto.Email))
                throw new ArgumentException("Invalid email format", nameof(supplierDto.Email));
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        //DTO to Entity and Entity to DTO mapping methods
        private static SupplierDTO MapToDTO(Supplier supplier)
        {
            return new SupplierDTO
            {
                SupplierID = supplier.SupplierID,
                SupplierName = supplier.SupplierName,
                ContactPerson = supplier.ContactPerson,
                PhoneNumber = supplier.PhoneNumber,
                Email = supplier.Email,
                Address = supplier.Address,
                CreatedDate = supplier.CreatedDate,
                UpdatedDate = supplier.UpdatedDate,
                CreatedBy = supplier.CreatedBy,
                IsActive = supplier.IsActive
            };
        }

        private static Supplier MapToEntity(SupplierDTO supplierDto)
        {
            return new Supplier
            {
                SupplierID = supplierDto.SupplierID,
                SupplierName = supplierDto.SupplierName,
                ContactPerson = supplierDto.ContactPerson,
                PhoneNumber = supplierDto.PhoneNumber,
                Email = supplierDto.Email,
                Address = supplierDto.Address,
                CreatedDate = supplierDto.CreatedDate,
                UpdatedDate = supplierDto.UpdatedDate,
                CreatedBy = supplierDto.CreatedBy,
                IsActive = supplierDto.IsActive
            };
        }
    }
}
