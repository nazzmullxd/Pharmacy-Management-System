using System;

namespace Business.DTO
{
    public class SupportTicketDTO
    {
        public Guid TicketID { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "System Bug", "Inventory Issue", "Billing Problem", "Feature Request"
        public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High", "Critical"
        public string Status { get; set; } = "Open"; // "Open", "In Progress", "Resolved", "Closed"
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public Guid? AssignedTo { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
        public DateTime? AssignedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public bool IsInternal { get; set; } = true;
    }
}
