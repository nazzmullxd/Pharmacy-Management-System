using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class Product
    {
        [Key]
        public Guid ProductID { get; set; } = Guid.NewGuid();

        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public string GenericName { get; set; } = string.Empty;

        [Required]
        public string Manufacturer { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal UnitPrice { get; set; } = 0.0m;

        [Required]
        public decimal DefaultRetailPrice { get; set; } = 0.0m;

        [Required]
        public decimal DefaultWholeSalePrice { get; set; } = 0.0m;

        public bool IsActive { get; set; } = true;

        public string Barcode { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property for related ProductBatches
        public ICollection<ProductBatch>? ProductBatches { get; set; }
    }
}