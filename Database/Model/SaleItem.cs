using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class SaleItem
    {
        [Key]
        public string SalesItemID { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string ProductBatchID { get; set; } = string.Empty;
        [Required]
        public string SaleID { get; set; } = string.Empty;
        [Required]
        public int Quantity { get; set; } = 0;
        [Required]
        public decimal UnitPrice { get; set; } = 0.0m;
        [Required]
        public string ProductID { get; set; } = string.Empty;
        public string? BatchID { get; set; } = null;
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    }
}
