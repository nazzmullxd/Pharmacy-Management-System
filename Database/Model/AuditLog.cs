using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class AuditLog
    {
        // Primary Key
        [Key]
        public Guid AuditLogID { get; set; } = Guid.NewGuid();

        // Foreign Key
        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;

        // Navigation Property
        public User? User { get; set; }

        // Value Properties
        [Required]
        public string Action { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        // Metadata
        [Required]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    }
}