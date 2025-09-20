using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class Sale
    {
        [Key]
        public Guid SaleID { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(Customer))]
        public Guid CustomerID { get; set; } = Guid.Empty;

        public Customer? Customer { get; set; }

        [Required]
        [ForeignKey(nameof(UserInfo))]
        public Guid UserID { get; set; } = Guid.Empty;

        public UserInfo? User { get; set; }
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

        [Required]
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be a positive value.")]
        public decimal TotalAmount { get; set; } = 0.0m;

        [Required]
        [MaxLength(20, ErrorMessage = "Payment status cannot exceed 20 characters.")]
        public string PaymentStatus { get; set; } = "Paid"; // Example statuses: Pending, Paid, Failed

        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
        public string Note { get; set; } = string.Empty;
    }
}