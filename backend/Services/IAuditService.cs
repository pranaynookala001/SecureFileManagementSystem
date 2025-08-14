using SecureDocumentAPI.DTOs;

namespace SecureDocumentAPI.Services;

public interface IAuditService
{
    Task LogActivityAsync(
        string userId,
        string action,
        string entityType,
        string description,
        string severity = "Info",
        Guid? entityId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? sessionId = null,
        string? details = null,
        string? resource = null,
        string? status = null,
        string? errorMessage = null,
        bool isSecurityEvent = false,
        bool requiresReview = false
    );
    
    Task<List<AuditLogDto>> GetAuditLogsAsync(
        string? userId = null,
        string? action = null,
        string? entityType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? severity = null,
        bool? isSecurityEvent = null,
        int page = 1,
        int pageSize = 50
    );
    
    Task<List<AuditLogDto>> GetSecurityEventsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? severity = null,
        bool? requiresReview = null,
        int page = 1,
        int pageSize = 50
    );
    
    Task<bool> ReviewAuditLogAsync(Guid auditLogId, string reviewedBy, string reviewNotes);
    
    Task<AuditSummaryDto> GetAuditSummaryAsync(DateTime fromDate, DateTime toDate);
    
    Task<List<AuditLogDto>> GetUserActivityAsync(string userId, DateTime fromDate, DateTime toDate);
    
    Task<List<AuditLogDto>> GetDocumentActivityAsync(Guid documentId, DateTime fromDate, DateTime toDate);
    
    Task<bool> ExportAuditLogsAsync(DateTime fromDate, DateTime toDate, string format = "csv");
    
    Task<bool> ArchiveAuditLogsAsync(DateTime beforeDate);
    
    Task<int> GetAuditLogCountAsync(DateTime fromDate, DateTime toDate);
    
    Task<List<string>> GetDistinctActionsAsync();
    
    Task<List<string>> GetDistinctEntityTypesAsync();
    
    Task<List<string>> GetDistinctSeveritiesAsync();
}
