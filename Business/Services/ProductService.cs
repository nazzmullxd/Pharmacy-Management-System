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
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product?> GetProductByIdAsync(Guid productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

        public async Task AddProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null");
            }

            ValidateProduct(product);

            await _productRepository.AddAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null");
            }

            ValidateProduct(product);

            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            await _productRepository.DeleteAsync(productId);
        }

        public async Task<IEnumerable<Product>> SearchProductsByNameAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new ArgumentException("Product name cannot be null or empty", nameof(productName));
            }

            return await _productRepository.GetByNameAsync(productName);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category cannot be null or empty", nameof(category));
            }

            return await _productRepository.GetByCategoryAsync(category);
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _productRepository.GetActiveProductsAsync();
        }

        private void ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                throw new ArgumentException("Product name is required", nameof(product.ProductName));
            }

            if (string.IsNullOrWhiteSpace(product.GenericName))
            {
                throw new ArgumentException("Generic name is required", nameof(product.GenericName));
            }

            if (string.IsNullOrWhiteSpace(product.Manufacturer))
            {
                throw new ArgumentException("Manufacturer is required", nameof(product.Manufacturer));
            }

            if (string.IsNullOrWhiteSpace(product.Category))
            {
                throw new ArgumentException("Category is required", nameof(product.Category));
            }

            if (product.UnitPrice <= 0)
            {
                throw new ArgumentException("Unit price must be greater than zero", nameof(product.UnitPrice));
            }

            if (product.DefaultRetailPrice <= 0)
            {
                throw new ArgumentException("Default retail price must be greater than zero", nameof(product.DefaultRetailPrice));
            }

            if (product.DefaultWholeSalePrice <= 0)
            {
                throw new ArgumentException("Default wholesale price must be greater than zero", nameof(product.DefaultWholeSalePrice));
            }
        }

        // IProductService implementation
        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(MapToDTO);
        }

        public async Task<ProductDTO?> GetProductByIdAsync(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product != null ? MapToDTO(product) : null;
        }

        public async Task<ProductDTO> CreateProductAsync(ProductDTO productDto)
        {
            if (productDto == null)
                throw new ArgumentNullException(nameof(productDto));

            var product = MapToEntity(productDto);
            product.ProductID = Guid.NewGuid();
            product.CreatedDate = DateTime.UtcNow;

            ValidateProduct(product);
            await _productRepository.AddAsync(product);
            return MapToDTO(product);
        }

        public async Task<ProductDTO> UpdateProductAsync(ProductDTO productDto)
        {
            if (productDto == null)
                throw new ArgumentNullException(nameof(productDto));

            var existingProduct = await _productRepository.GetByIdAsync(productDto.ProductID);
            if (existingProduct == null)
                throw new ArgumentException("Product not found", nameof(productDto.ProductID));

            var product = MapToEntity(productDto);
            product.CreatedDate = existingProduct.CreatedDate; // Preserve original creation date

            ValidateProduct(product);
            await _productRepository.UpdateAsync(product);
            return MapToDTO(product);
        }

        public async Task<bool> DeleteProductAsync(Guid productId)
        {
            try
            {
                await _productRepository.DeleteAsync(productId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<ProductDTO>> SearchProductsByNameAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name cannot be null or empty", nameof(productName));

            var products = await _productRepository.GetByNameAsync(productName);
            return products.Select(MapToDTO);
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be null or empty", nameof(category));

            var products = await _productRepository.GetByCategoryAsync(category);
            return products.Select(MapToDTO);
        }

        public async Task<IEnumerable<ProductDTO>> GetActiveProductsAsync()
        {
            var products = await _productRepository.GetActiveProductsAsync();
            return products.Select(MapToDTO);
        }

        public async Task<IEnumerable<ProductDTO>> GetLowStockProductsAsync(int threshold = 10)
        {
            // This would need to be implemented with stock calculation
            // For now, returning empty list as stock calculation requires ProductBatch integration
            return new List<ProductDTO>();
        }

        public async Task<bool> IsBarcodeUniqueAsync(string barcode, Guid? excludeProductId = null)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return false;

            // This would need to be implemented in the repository
            // For now, returning true as placeholder
            return true;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByManufacturerAsync(string manufacturer)
        {
            if (string.IsNullOrWhiteSpace(manufacturer))
                throw new ArgumentException("Manufacturer cannot be null or empty", nameof(manufacturer));

            var products = await _productRepository.GetAllAsync();
            return products.Where(p => p.Manufacturer.Contains(manufacturer, StringComparison.OrdinalIgnoreCase))
                          .Select(MapToDTO);
        }

        public async Task<bool> ToggleProductStatusAsync(Guid productId)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                    return false;

                product.IsActive = !product.IsActive;
                await _productRepository.UpdateAsync(product);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Helper methods
        private static ProductDTO MapToDTO(Product product)
        {
            return new ProductDTO
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                GenericName = product.GenericName,
                Manufacturer = product.Manufacturer,
                Category = product.Category,
                Description = product.Description,
                UnitPrice = product.UnitPrice,
                DefaultRetailPrice = product.DefaultRetailPrice,
                DefaultWholeSalePrice = product.DefaultWholeSalePrice,
                IsActive = product.IsActive,
                Barcode = product.Barcode,
                CreatedDate = product.CreatedDate,
                TotalStock = 0 // Would need to be calculated from ProductBatches
            };
        }

        private static Product MapToEntity(ProductDTO productDto)
        {
            return new Product
            {
                ProductID = productDto.ProductID,
                ProductName = productDto.ProductName,
                GenericName = productDto.GenericName,
                Manufacturer = productDto.Manufacturer,
                Category = productDto.Category,
                Description = productDto.Description,
                UnitPrice = productDto.UnitPrice,
                DefaultRetailPrice = productDto.DefaultRetailPrice,
                DefaultWholeSalePrice = productDto.DefaultWholeSalePrice,
                IsActive = productDto.IsActive,
                Barcode = productDto.Barcode,
                CreatedDate = productDto.CreatedDate
            };
        }
    }
}