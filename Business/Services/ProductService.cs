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
            ValidateProduct(product);
            await _productRepository.AddAsync(product);
            return MapToDTO(product);
        }

        public async Task<ProductDTO> UpdateProductAsync(ProductDTO productDto)
        {
            if (productDto == null)
                throw new ArgumentNullException(nameof(productDto));

            var product = MapToEntity(productDto);
            ValidateProduct(product);
            await _productRepository.UpdateAsync(product);
            return MapToDTO(product);
        }

        public async Task<bool> DeleteProductAsync(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Product ID cannot be empty", nameof(productId));

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;

            await _productRepository.DeleteAsync(productId);
            return true;
        }

        public async Task<IEnumerable<ProductDTO>> SearchProductsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Search name cannot be null or empty", nameof(name));

            var products = await _productRepository.GetByNameAsync(name);
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
            var products = await _productRepository.GetAllAsync();
            return products.Where(p => p.TotalStock <= threshold).Select(MapToDTO);
        }

        public async Task<bool> IsBarcodeUniqueAsync(string barcode, Guid? excludeProductId = null)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return true;

            var products = await _productRepository.GetAllAsync();
            return !products.Any(p => p.Barcode == barcode && p.ProductID != excludeProductId);
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByManufacturerAsync(string manufacturer)
        {
            if (string.IsNullOrWhiteSpace(manufacturer))
                throw new ArgumentException("Manufacturer cannot be null or empty", nameof(manufacturer));

            var products = await _productRepository.GetAllAsync();
            return products.Where(p => p.Manufacturer.Equals(manufacturer, StringComparison.OrdinalIgnoreCase)).Select(MapToDTO);
        }

        public async Task<bool> ToggleProductStatusAsync(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;

            product.IsActive = !product.IsActive;
            await _productRepository.UpdateAsync(product);
            return true;
        }

        private ProductDTO MapToDTO(Product product)
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
                TotalStock = product.TotalStock,
                LowStockThreshold = product.LowStockThreshold
            };
        }

        private Product MapToEntity(ProductDTO productDto)
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
                CreatedDate = productDto.CreatedDate,
                TotalStock = productDto.TotalStock,
                LowStockThreshold = productDto.LowStockThreshold
            };
        }

        private void ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new ArgumentException("Product name is required", nameof(product));

            if (product.UnitPrice <= 0)
                throw new ArgumentException("Unit price must be greater than 0", nameof(product));

            if (string.IsNullOrWhiteSpace(product.Category))
                throw new ArgumentException("Category is required", nameof(product));
        }
    }
}