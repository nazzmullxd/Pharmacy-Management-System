using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class AntibioticLog
    {
        // Primary Key
        [Key]
        public Guid AntibioticLogID { get; set; } = Guid.NewGuid();

        // Foreign Keys
        [Required]
        [ForeignKey(nameof(Sale))]
        public string SaleID { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(ProductBatch))]
        public string ProductBatchID { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(Product))]
        public string ProductID { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(Customer))]
        public string CustomerID { get; set; } = string.Empty;

        // Navigation Properties
        public Sale? Sale { get; set; }
        public ProductBatch? ProductBatch { get; set; }
        public Product? Product { get; set; }
        public Customer? Customer { get; set; }

        // Value Properties
        [Required]
        public string DoctorName { get; set; } = string.Empty;

        [Required]
        public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
    }
}