using SecureDocumentAPI.DTOs;

namespace SecureDocumentAPI.Services;

public interface IDocumentService
{
    Task<DocumentDto> UploadDocumentAsync(UploadDocumentRequestDto request, string userId);
    Task<DocumentDto> GetDocumentAsync(Guid documentId, string userId);
    Task<byte[]> DownloadDocumentAsync(Guid documentId, string userId);
    Task<List<DocumentDto>> GetUserDocumentsAsync(string userId, int page = 1, int pageSize = 20);
    Task<bool> UpdateDocumentAsync(Guid documentId, UpdateDocumentRequestDto request, string userId);
    Task<bool> DeleteDocumentAsync(Guid documentId, string userId);
    Task<bool> ShareDocumentAsync(Guid documentId, ShareDocumentRequestDto request, string userId);
    Task<List<DocumentPermissionDto>> GetDocumentPermissionsAsync(Guid documentId, string userId);
    Task<bool> UpdateDocumentPermissionAsync(Guid documentId, Guid userId, UpdatePermissionRequestDto request);
    Task<bool> RemoveDocumentPermissionAsync(Guid documentId, Guid userId, string ownerId);
    Task<DocumentDto> CreateDocumentVersionAsync(Guid documentId, UploadDocumentRequestDto request, string userId);
    Task<List<DocumentDto>> GetDocumentVersionsAsync(Guid documentId, string userId);
    Task<bool> ArchiveDocumentAsync(Guid documentId, string userId);
    Task<bool> RestoreDocumentAsync(Guid documentId, string userId);
    Task<List<DocumentDto>> SearchDocumentsAsync(string searchTerm, string userId, int page = 1, int pageSize = 20);
    Task<List<DocumentDto>> GetDocumentsByTagAsync(string tag, string userId, int page = 1, int pageSize = 20);
    Task<bool> AddDocumentTagAsync(Guid documentId, string tag, string userId);
    Task<bool> RemoveDocumentTagAsync(Guid documentId, string tag, string userId);
    Task<List<string>> GetDocumentTagsAsync(Guid documentId, string userId);
    Task<DocumentStatisticsDto> GetDocumentStatisticsAsync(string userId);
}
