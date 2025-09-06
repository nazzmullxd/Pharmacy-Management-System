using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;
using Database.Model;

namespace Business.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO?> GetProductByIdAsync(Guid productId);
        Task<ProductDTO> CreateProductAsync(ProductDTO productDto);
        Task<ProductDTO> UpdateProductAsync(ProductDTO productDto);
        Task<bool> DeleteProductAsync(Guid productId);
        Task<IEnumerable<ProductDTO>> SearchProductsByNameAsync(string productName);
        Task<IEnumerable<ProductDTO>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<ProductDTO>> GetActiveProductsAsync();
        Task<IEnumerable<ProductDTO>> GetLowStockProductsAsync(int threshold = 10);
        Task<bool> IsBarcodeUniqueAsync(string barcode, Guid? excludeProductId = null);
        Task<IEnumerable<ProductDTO>> GetProductsByManufacturerAsync(string manufacturer);
        Task<bool> ToggleProductStatusAsync(Guid productId);
    }
}
