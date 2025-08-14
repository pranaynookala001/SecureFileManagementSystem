using System.ComponentModel.DataAnnotations;

namespace SecureDocumentAPI.DTOs
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string EncryptionKey { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public DateTime? LastModified { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public List<DocumentPermissionDto> Permissions { get; set; } = new List<DocumentPermissionDto>();
    }

    public class UploadDocumentRequestDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string FileType { get; set; } = string.Empty;
        
        [Required]
        public long FileSize { get; set; }
        
        public List<string> Tags { get; set; } = new List<string>();
        
        public string FolderId { get; set; } = string.Empty;
    }

    public class UpdateDocumentRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public string FolderId { get; set; } = string.Empty;
    }

    public class ShareDocumentRequestDto
    {
        [Required]
        public string UserEmail { get; set; } = string.Empty;
        
        [Required]
        public string Permission { get; set; } = string.Empty; // Read, Write, Admin
        
        public DateTime? ExpiresAt { get; set; }
    }

    public class DocumentPermissionDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string GrantedBy { get; set; } = string.Empty;
    }

    public class UpdatePermissionRequestDto
    {
        [Required]
        public string Permission { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }

    public class DocumentMetadataDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class DocumentStatisticsDto
    {
        public int TotalDocuments { get; set; }
        public long TotalSize { get; set; }
        public Dictionary<string, int> DocumentsByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> DocumentsByStatus { get; set; } = new Dictionary<string, int>();
        public List<DocumentMetadataDto> RecentDocuments { get; set; } = new List<DocumentMetadataDto>();
    }
}
