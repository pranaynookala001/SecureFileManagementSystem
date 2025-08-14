using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureDocumentAPI.Models;

public class SecurityEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty; // LoginAttempt, AccessDenied, DataExfiltration, etc.
    
    [Required]
    [StringLength(50)]
    public string Severity { get; set; } = "Info"; // Info, Warning, Error, Critical
    
    [Required]
    [StringLength(100)]
    public string Source { get; set; } = string.Empty; // API, Web, Mobile, System
    
    [StringLength(100)]
    public string? UserId { get; set; }
    
    [StringLength(100)]
    public string? Username { get; set; }
    
    [StringLength(45)]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    public string? UserAgent { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [StringLength(2000)]
    public string? Details { get; set; } // JSON serialized additional data
    
    [StringLength(100)]
    public string? Resource { get; set; } // API endpoint, file path, etc.
    
    [StringLength(100)]
    public string? SessionId { get; set; }
    
    [Required]
    public bool IsResolved { get; set; } = false;
    
    public DateTime? ResolvedAt { get; set; }
    
    [StringLength(100)]
    public string? ResolvedBy { get; set; }
    
    [StringLength(1000)]
    public string? ResolutionNotes { get; set; }
    
    [Required]
    public bool RequiresImmediateAction { get; set; } = false;
    
    public DateTime? ActionTakenAt { get; set; }
    
    [StringLength(100)]
    public string? ActionTakenBy { get; set; }
    
    [StringLength(1000)]
    public string? ActionTaken { get; set; }
    
    [StringLength(50)]
    public string? ThreatLevel { get; set; } // Low, Medium, High, Critical
    
    [StringLength(100)]
    public string? Category { get; set; } // Authentication, Authorization, Data, Network, etc.
    
    // Navigation properties
    public Guid? UserSecurityId { get; set; }
    
    [ForeignKey("UserSecurityId")]
    public virtual User? User { get; set; }
    
    // Methods
    public bool IsHighSeverity()
    {
        return Severity is "Error" or "Critical";
    }
    
    public bool IsCritical()
    {
        return Severity == "Critical" || ThreatLevel == "Critical";
    }
    
    public bool NeedsImmediateAttention()
    {
        return RequiresImmediateAction || IsCritical();
    }
    
    public string GetFormattedTimestamp()
    {
        return Timestamp.ToString("yyyy-MM-dd HH:mm:ss UTC");
    }
    
    public string GetEventSummary()
    {
        return $"{EventType} - {Severity} - {Description}";
    }
    
    public bool IsAuthenticationRelated()
    {
        return EventType.Contains("Login") || EventType.Contains("Auth") || 
               EventType.Contains("Password") || EventType.Contains("Session");
    }
    
    public bool IsDataRelated()
    {
        return EventType.Contains("Data") || EventType.Contains("Document") || 
               EventType.Contains("File") || EventType.Contains("Access");
    }
}
