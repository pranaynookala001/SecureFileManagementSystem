using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureDocumentAPI.Models;

public class AuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty; // Login, Logout, Create, Read, Update, Delete, etc.
    
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty; // User, Document, Folder, etc.
    
    public Guid? EntityId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Username { get; set; }
    
    [StringLength(100)]
    public string? UserEmail { get; set; }
    
    [StringLength(50)]
    public string? UserRole { get; set; }
    
    [StringLength(45)]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    public string? UserAgent { get; set; }
    
    [StringLength(100)]
    public string? SessionId { get; set; }
    
    [StringLength(50)]
    public string? Severity { get; set; } = "Info"; // Info, Warning, Error, Critical
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [StringLength(1000)]
    public string? Details { get; set; } // JSON serialized additional data
    
    [StringLength(100)]
    public string? Resource { get; set; } // API endpoint, file path, etc.
    
    [StringLength(50)]
    public string? Status { get; set; } = "Success"; // Success, Failure, Pending
    
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    public bool IsSecurityEvent { get; set; } = false;
    
    public bool RequiresReview { get; set; } = false;
    
    public DateTime? ReviewedAt { get; set; }
    
    [StringLength(100)]
    public string? ReviewedBy { get; set; }
    
    [StringLength(1000)]
    public string? ReviewNotes { get; set; }
    
    // Foreign keys
    public Guid? DocumentId { get; set; }
    
    public Guid? UserAuditId { get; set; }
    
    // Navigation properties
    [ForeignKey("DocumentId")]
    public virtual Document? Document { get; set; }
    
    [ForeignKey("UserAuditId")]
    public virtual User? User { get; set; }
    
    // Methods
    public bool IsHighSeverity()
    {
        return Severity is "Error" or "Critical";
    }
    
    public bool IsSecurityRelated()
    {
        return IsSecurityEvent || Action.Contains("Login") || Action.Contains("Logout") || 
               Action.Contains("Permission") || Action.Contains("Access") || 
               Action.Contains("Security") || Action.Contains("Encryption");
    }
    
    public bool NeedsImmediateAttention()
    {
        return IsHighSeverity() && IsSecurityRelated();
    }
    
    public string GetFormattedTimestamp()
    {
        return Timestamp.ToString("yyyy-MM-dd HH:mm:ss UTC");
    }
    
    public string GetSummary()
    {
        return $"{Action} on {EntityType} by {Username ?? UserId} at {GetFormattedTimestamp()}";
    }
}
