using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureDocumentAPI.Models;

public class DocumentTag
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid DocumentId { get; set; }
    
    [Required]
    public Guid TagId { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [StringLength(100)]
    public string? AddedBy { get; set; }
    
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;
    
    [ForeignKey("TagId")]
    public virtual Tag Tag { get; set; } = null!;
}
