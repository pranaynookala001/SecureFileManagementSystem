using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureDocumentAPI.Models;

public class DocumentComment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid DocumentId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public Guid? ParentCommentId { get; set; }
    
    [Required]
    public bool IsResolved { get; set; } = false;
    
    [Required]
    public bool IsPrivate { get; set; } = false;
    
    public DateTime? ResolvedAt { get; set; }
    
    [StringLength(100)]
    public string? ResolvedBy { get; set; }
    
    public int LineNumber { get; set; } = 0; // For line-specific comments
    
    [StringLength(100)]
    public string? PageNumber { get; set; } // For page-specific comments
    
    [StringLength(500)]
    public string? Coordinates { get; set; } // For position-specific comments (JSON)
    
    // Navigation properties
    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    [ForeignKey("ParentCommentId")]
    public virtual DocumentComment? ParentComment { get; set; }
    
    public virtual ICollection<DocumentComment> Replies { get; set; } = new List<DocumentComment>();
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Methods
    public bool IsReply()
    {
        return ParentCommentId.HasValue;
    }
    
    public bool HasReplies()
    {
        return Replies.Any();
    }
    
    public int GetReplyCount()
    {
        return Replies.Count;
    }
    
    public string GetFormattedContent()
    {
        return Content.Replace("\n", "<br>");
    }
    
    public bool CanBeViewedBy(User user)
    {
        if (user.Role == "Admin") return true;
        if (UserId == user.Id) return true;
        if (!IsPrivate) return true;
        
        // Check if user has permission to view the document
        return Document.CanBeAccessedBy(user);
    }
}
