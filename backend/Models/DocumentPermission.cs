using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureDocumentAPI.Models;

public class DocumentPermission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid DocumentId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public bool CanView { get; set; } = false;
    
    [Required]
    public bool CanEdit { get; set; } = false;
    
    [Required]
    public bool CanDelete { get; set; } = false;
    
    [Required]
    public bool CanShare { get; set; } = false;
    
    [Required]
    public bool CanDownload { get; set; } = false;
    
    [Required]
    public bool CanComment { get; set; } = false;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public DateTime? ExpiresAt { get; set; }
    
    [StringLength(500)]
    public string? GrantedBy { get; set; }
    
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    // Navigation properties
    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    // Methods
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
    
    public bool HasAnyPermission()
    {
        return CanView || CanEdit || CanDelete || CanShare || CanDownload || CanComment;
    }
    
    public string GetPermissionSummary()
    {
        var permissions = new List<string>();
        if (CanView) permissions.Add("View");
        if (CanEdit) permissions.Add("Edit");
        if (CanDelete) permissions.Add("Delete");
        if (CanShare) permissions.Add("Share");
        if (CanDownload) permissions.Add("Download");
        if (CanComment) permissions.Add("Comment");
        
        return string.Join(", ", permissions);
    }
}
