using System;

namespace Business.DTO
{
    public class ProductDTO
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal DefaultRetailPrice { get; set; }
        public decimal DefaultWholeSalePrice { get; set; }
        public bool IsActive { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int TotalStock { get; set; }
        public int LowStockThreshold { get; set; } = 10;
        public bool IsLowStock => TotalStock <= LowStockThreshold;
    }
}
