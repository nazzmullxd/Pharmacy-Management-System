using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Business.DTO
{
    public class SaleDTO
    {
        public Guid SaleID { get; set; }
        public Guid CustomerID { get; set; }
        
        [CustomValidation(typeof(SaleDTO), nameof(ValidateCustomerName))]
        public string CustomerName { get; set; } = string.Empty;
        public Guid UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; } // Stored as UTC in persistence layer
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = PaymentStatuses.Paid;
        public string Note { get; set; } = string.Empty;
        public List<SaleItemDTO> SaleItems { get; set; } = new();

        // Custom validation method that allows empty CustomerName for walk-in customers
        public static ValidationResult? ValidateCustomerName(string customerName, ValidationContext context)
        {
            // Always allow empty customer name - walk-in customers are handled server-side
            return ValidationResult.Success;
        }

        // Centralized status constants (non-security)
        public static class PaymentStatuses
        {
            public const string Paid = "Paid";
            public const string Pending = "Pending";
            public const string Cancelled = "Cancelled";
            public const string Failed = "Failed";
        }
    }
}
