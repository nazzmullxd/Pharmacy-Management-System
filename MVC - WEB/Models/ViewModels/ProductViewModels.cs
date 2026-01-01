using Business.DTO;
using System.ComponentModel.DataAnnotations;

namespace MVC_WEB.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Products listing page
    /// </summary>
    public class ProductListViewModel
    {
        public List<ProductDTO> Products { get; set; } = new List<ProductDTO>();
        public int TotalProducts { get; set; }
        public int InStockCount { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }

    /// <summary>
    /// ViewModel for viewing product details
    /// </summary>
    public class ProductDetailsViewModel
    {
        public ProductDTO Product { get; set; } = new ProductDTO();
    }

    /// <summary>
    /// ViewModel for creating a new product
    /// </summary>
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Generic name is required")]
        [StringLength(200, ErrorMessage = "Generic name cannot exceed 200 characters")]
        [Display(Name = "Generic Name")]
        public string GenericName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Manufacturer is required")]
        [StringLength(200, ErrorMessage = "Manufacturer cannot exceed 200 characters")]
        [Display(Name = "Manufacturer")]
        public string Manufacturer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Unit price must be between 0.01 and 999,999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Unit Price (Cost)")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Retail price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Retail price must be between 0.01 and 999,999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Retail Price")]
        public decimal DefaultRetailPrice { get; set; }

        [Required(ErrorMessage = "Wholesale price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Wholesale price must be between 0.01 and 999,999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Wholesale Price")]
        public decimal DefaultWholeSalePrice { get; set; }

        [StringLength(50, ErrorMessage = "Barcode cannot exceed 50 characters")]
        [Display(Name = "Barcode")]
        public string? Barcode { get; set; }

        [Range(0, 10000, ErrorMessage = "Low stock threshold must be between 0 and 10,000")]
        [Display(Name = "Low Stock Threshold")]
        public int LowStockThreshold { get; set; } = 10;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<string> Categories { get; set; } = new List<string>();
    }

    /// <summary>
    /// ViewModel for editing an existing product
    /// </summary>
    public class ProductEditViewModel
    {
        public Guid ProductID { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Generic name is required")]
        [StringLength(200, ErrorMessage = "Generic name cannot exceed 200 characters")]
        [Display(Name = "Generic Name")]
        public string GenericName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Manufacturer is required")]
        [StringLength(200, ErrorMessage = "Manufacturer cannot exceed 200 characters")]
        [Display(Name = "Manufacturer")]
        public string Manufacturer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Unit price must be between 0.01 and 999,999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Unit Price (Cost)")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Retail price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Retail price must be between 0.01 and 999,999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Retail Price")]
        public decimal DefaultRetailPrice { get; set; }

        [Required(ErrorMessage = "Wholesale price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Wholesale price must be between 0.01 and 999,999.99")]
        [DataType(DataType.Currency)]
        [Display(Name = "Wholesale Price")]
        public decimal DefaultWholeSalePrice { get; set; }

        [StringLength(50, ErrorMessage = "Barcode cannot exceed 50 characters")]
        [Display(Name = "Barcode")]
        public string? Barcode { get; set; }

        [Range(0, 10000, ErrorMessage = "Low stock threshold must be between 0 and 10,000")]
        [Display(Name = "Low Stock Threshold")]
        public int LowStockThreshold { get; set; } = 10;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Total Stock")]
        public int TotalStock { get; set; }

        public List<string> Categories { get; set; } = new List<string>();
    }

    /// <summary>
    /// ViewModel for the delete confirmation page
    /// </summary>
    public class ProductDeleteViewModel
    {
        public ProductDTO Product { get; set; } = new ProductDTO();
    }

    /// <summary>
    /// ViewModel for the categories page
    /// </summary>
    public class ProductCategoriesViewModel
    {
        public List<string> Categories { get; set; } = new List<string>();
        public Dictionary<string, int> CategoryCounts { get; set; } = new Dictionary<string, int>();
        public bool IsAdmin { get; set; }
    }
}
