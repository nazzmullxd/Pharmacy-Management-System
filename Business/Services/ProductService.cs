using Database.Interfaces;
using Database.Model;
using Database.Repositories;
namespace Business.Services
{
    public class ProductService
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
    }
}