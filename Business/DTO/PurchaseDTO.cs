using System;
using System.Collections.Generic;

namespace Business.DTO
{
    public class PurchaseDTO
    {
        public Guid PurchaseID { get; set; }
        public Guid SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public Guid UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string Note { get; set; } = string.Empty;
        public List<PurchaseItemDTO> PurchaseItems { get; set; } = new List<PurchaseItemDTO>();
    }
}
