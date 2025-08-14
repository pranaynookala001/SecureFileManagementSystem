using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureDocumentAPI.Models;

public class Document
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string FileExtension { get; set; } = string.Empty;
    
    [Required]
    public long FileSize { get; set; }
    
    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string EncryptionKey { get; set; } = string.Empty; // Encrypted key
    
    [Required]
    [StringLength(100)]
    public string Checksum { get; set; } = string.Empty; // SHA-256 hash
    
    [Required]
    public int Version { get; set; } = 1;
    
    [Required]
    public bool IsLatestVersion { get; set; } = true;
    
    public Guid? PreviousVersionId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Active"; // Active, Archived, Deleted
    
    [Required]
    [StringLength(50)]
    public string SecurityLevel { get; set; } = "Internal"; // Public, Internal, Confidential, Secret
    
    [Required]
    public bool IsEncrypted { get; set; } = true;
    
    public DateTime? ExpiresAt { get; set; }
    
    public DateTime? LastAccessedAt { get; set; }
    
    public int AccessCount { get; set; } = 0;
    
    // Foreign keys
    [Required]
    public Guid OwnerId { get; set; }
    
    public Guid? FolderId { get; set; }
    
    // Navigation properties
    [ForeignKey("OwnerId")]
    public virtual User Owner { get; set; } = null!;
    
    [ForeignKey("FolderId")]
    public virtual Folder? Folder { get; set; }
    
    [ForeignKey("PreviousVersionId")]
    public virtual Document? PreviousVersion { get; set; }
    
    public virtual ICollection<Document> NextVersions { get; set; } = new List<Document>();
    
    public virtual ICollection<DocumentPermission> Permissions { get; set; } = new List<DocumentPermission>();
    
    public virtual ICollection<DocumentComment> Comments { get; set; } = new List<DocumentComment>();
    
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    
    public virtual ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Methods
    public string GetFullFileName()
    {
        return $"{FileName}.{FileExtension}";
    }
    
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
    
    public bool RequiresSpecialAccess()
    {
        return SecurityLevel is "Confidential" or "Secret";
    }
    
    public void IncrementAccessCount()
    {
        AccessCount++;
        LastAccessedAt = DateTime.UtcNow;
    }
    
    public bool CanBeAccessedBy(User user)
    {
        if (user.Role == "Admin") return true;
        if (OwnerId == user.Id) return true;
        
        var permission = Permissions.FirstOrDefault(p => p.UserId == user.Id && p.IsActive);
        return permission != null;
    }
    
    public bool CanBeEditedBy(User user)
    {
        if (user.Role == "Admin") return true;
        if (OwnerId == user.Id) return true;
        
        var permission = Permissions.FirstOrDefault(p => p.UserId == user.Id && p.IsActive);
        return permission?.CanEdit == true;
    }
}
