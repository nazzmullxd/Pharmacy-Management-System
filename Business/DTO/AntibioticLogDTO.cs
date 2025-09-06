using System;

namespace Business.DTO
{
    public class AntibioticLogDTO
    {
        public Guid AntibioticLogID { get; set; }
        public Guid SaleID { get; set; }
        public Guid ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerContact { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime SaleDate { get; set; }
        public Guid UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
    }
}
