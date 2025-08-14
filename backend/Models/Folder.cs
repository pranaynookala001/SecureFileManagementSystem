using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureDocumentAPI.Models;

public class Folder
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public Guid? ParentId { get; set; }
    
    [Required]
    public Guid OwnerId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Active"; // Active, Archived, Deleted
    
    [Required]
    [StringLength(50)]
    public string SecurityLevel { get; set; } = "Internal"; // Public, Internal, Confidential, Secret
    
    public int DocumentCount { get; set; } = 0;
    
    public long TotalSize { get; set; } = 0;
    
    // Navigation properties
    [ForeignKey("ParentId")]
    public virtual Folder? Parent { get; set; }
    
    public virtual ICollection<Folder> Subfolders { get; set; } = new List<Folder>();
    
    [ForeignKey("OwnerId")]
    public virtual User Owner { get; set; } = null!;
    
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Methods
    public string GetFullPath()
    {
        var path = new List<string> { Name };
        var current = Parent;
        
        while (current != null)
        {
            path.Insert(0, current.Name);
            current = current.Parent;
        }
        
        return string.Join("/", path);
    }
    
    public bool IsRoot()
    {
        return ParentId == null;
    }
    
    public int GetDepth()
    {
        var depth = 0;
        var current = Parent;
        
        while (current != null)
        {
            depth++;
            current = current.Parent;
        }
        
        return depth;
    }
    
    public bool CanBeAccessedBy(User user)
    {
        if (user.Role == "Admin") return true;
        if (OwnerId == user.Id) return true;
        
        // Check if user has access to any documents in this folder
        return Documents.Any(d => d.CanBeAccessedBy(user));
    }
}
