using System.ComponentModel.DataAnnotations;

namespace SecureDocumentAPI.Models;

public class Tag
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(7)]
    public string? Color { get; set; } = "#1976d2"; // Hex color code
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public int UsageCount { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Methods
    public void IncrementUsageCount()
    {
        UsageCount++;
    }
    
    public void DecrementUsageCount()
    {
        if (UsageCount > 0)
            UsageCount--;
    }
}
