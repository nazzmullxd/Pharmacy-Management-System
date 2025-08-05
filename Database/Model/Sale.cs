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
        [ForeignKey(nameof(User))]
        public Guid userID { get; set; } = Guid.Empty;
        public User? User { get; set; }
        [Required]
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        [Required]
        public decimal TotalAmount { get; set; } = 0.0m;
        [Required]
        public string PaymentSatus { get; set; } = "Paid"; // Example statuses: Pending, Paid, Failed
        public string Note { get; set; } = string.Empty;


    }
}
