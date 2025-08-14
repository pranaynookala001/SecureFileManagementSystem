using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureDocumentAPI.Models;

public class UserSession
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string SessionToken { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime ExpiresAt { get; set; }
    
    [StringLength(45)]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    public string? UserAgent { get; set; }
    
    [StringLength(100)]
    public string? DeviceInfo { get; set; }
    
    [StringLength(100)]
    public string? Location { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastActivityAt { get; set; }
    
    [StringLength(50)]
    public string? SessionType { get; set; } = "Web"; // Web, Mobile, API
    
    [StringLength(1000)]
    public string? AdditionalData { get; set; } // JSON serialized data
    
    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    // Methods
    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }
    
    public bool IsValid()
    {
        return IsActive && !IsExpired();
    }
    
    public TimeSpan GetRemainingTime()
    {
        if (IsExpired())
            return TimeSpan.Zero;
            
        return ExpiresAt - DateTime.UtcNow;
    }
    
    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }
    
    public string GetSessionSummary()
    {
        return $"Session {Id} for {User?.Username ?? UserId.ToString()} - {SessionType} from {IpAddress}";
    }
}
