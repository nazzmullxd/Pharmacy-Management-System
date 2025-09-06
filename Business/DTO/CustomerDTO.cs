using System;

namespace Business.DTO
{
    public class CustomerDTO
    {
        public Guid CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
