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

       /* // Foreign Key
        [Required]
        [ForeignKey(nameof(UserInfo))]
        public Guid UserID { get; set; } = Guid.Empty;
       */
        // Navigation Property
        public UserInfo? User { get; set; }

        // Value Properties
        [Required]
        public string Action { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;


        // Metadata
        [Required]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

    }
}