using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureDocumentAPI.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Viewer"; // Admin, Manager, Editor, Viewer
    
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool EmailVerified { get; set; } = false;
    
    public bool TwoFactorEnabled { get; set; } = false;
    
    [StringLength(255)]
    public string? TwoFactorSecret { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    
    public DateTime? PasswordChangedAt { get; set; }
    
    public int FailedLoginAttempts { get; set; } = 0;
    
    public DateTime? LockedUntil { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = "System";
    
    // Navigation properties
    public virtual ICollection<Document> OwnedDocuments { get; set; } = new List<Document>();
    
    public virtual ICollection<DocumentPermission> DocumentPermissions { get; set; } = new List<DocumentPermission>();
    
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    
    // Methods
    public string GetFullName()
    {
        return $"{FirstName} {LastName}".Trim();
    }
    
    public bool IsLocked()
    {
        return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }
    
    public bool CanAccessDocument(Document document)
    {
        if (Role == "Admin") return true;
        if (document.OwnerId == Id) return true;
        
        return DocumentPermissions.Any(dp => dp.DocumentId == document.Id && dp.IsActive);
    }
    
    public bool HasPermission(string permission)
    {
        return Role switch
        {
            "Admin" => true,
            "Manager" => permission is not "admin" and not "system",
            "Editor" => permission is "view" or "edit" or "comment",
            "Viewer" => permission == "view",
            _ => false
        };
    }
}
