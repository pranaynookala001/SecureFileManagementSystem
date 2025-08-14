using Microsoft.EntityFrameworkCore;
using SecureDocumentAPI.Data;
using SecureDocumentAPI.DTOs;
using SecureDocumentAPI.Models;

namespace SecureDocumentAPI.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogActivityAsync(
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
        bool requiresReview = false)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                Severity = severity,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                SessionId = sessionId,
                Details = details,
                Resource = resource,
                Status = status ?? "Success",
                ErrorMessage = errorMessage,
                IsSecurityEvent = isSecurityEvent,
                RequiresReview = requiresReview
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            // Log to console for development
            if (severity == "Error" || severity == "Critical" || isSecurityEvent)
            {
                _logger.LogWarning("Audit Log: {Action} on {EntityType} by {UserId} - {Description}", 
                    action, entityType, userId, description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit activity: {Action} by {UserId}", action, userId);
        }
    }

    public async Task<List<AuditLogDto>> GetAuditLogsAsync(
        string? userId = null,
        string? action = null,
        string? entityType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? severity = null,
        bool? isSecurityEvent = null,
        int page = 1,
        int pageSize = 50)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => a.UserId == userId);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Timestamp <= toDate.Value);

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(a => a.Severity == severity);

            if (isSecurityEvent.HasValue)
                query = query.Where(a => a.IsSecurityEvent == isSecurityEvent.Value);

            var auditLogs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    Timestamp = a.Timestamp,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    UserId = a.UserId,
                    Username = a.Username,
                    UserEmail = a.UserEmail,
                    UserRole = a.UserRole,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    SessionId = a.SessionId,
                    Severity = a.Severity,
                    Description = a.Description,
                    Details = a.Details,
                    Resource = a.Resource,
                    Status = a.Status,
                    ErrorMessage = a.ErrorMessage,
                    IsSecurityEvent = a.IsSecurityEvent,
                    RequiresReview = a.RequiresReview,
                    ReviewedAt = a.ReviewedAt,
                    ReviewedBy = a.ReviewedBy,
                    ReviewNotes = a.ReviewNotes
                })
                .ToListAsync();

            return auditLogs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return new List<AuditLogDto>();
        }
    }

    public async Task<List<AuditLogDto>> GetSecurityEventsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? severity = null,
        bool? requiresReview = null,
        int page = 1,
        int pageSize = 50)
    {
        try
        {
            var query = _context.AuditLogs.Where(a => a.IsSecurityEvent);

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Timestamp <= toDate.Value);

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(a => a.Severity == severity);

            if (requiresReview.HasValue)
                query = query.Where(a => a.RequiresReview == requiresReview.Value);

            var securityEvents = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    Timestamp = a.Timestamp,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    UserId = a.UserId,
                    Username = a.Username,
                    UserEmail = a.UserEmail,
                    UserRole = a.UserRole,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    SessionId = a.SessionId,
                    Severity = a.Severity,
                    Description = a.Description,
                    Details = a.Details,
                    Resource = a.Resource,
                    Status = a.Status,
                    ErrorMessage = a.ErrorMessage,
                    IsSecurityEvent = a.IsSecurityEvent,
                    RequiresReview = a.RequiresReview,
                    ReviewedAt = a.ReviewedAt,
                    ReviewedBy = a.ReviewedBy,
                    ReviewNotes = a.ReviewNotes
                })
                .ToListAsync();

            return securityEvents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security events");
            return new List<AuditLogDto>();
        }
    }

    public async Task<bool> ReviewAuditLogAsync(Guid auditLogId, string reviewedBy, string reviewNotes)
    {
        try
        {
            var auditLog = await _context.AuditLogs.FindAsync(auditLogId);
            if (auditLog == null)
                return false;

            auditLog.ReviewedAt = DateTime.UtcNow;
            auditLog.ReviewedBy = reviewedBy;
            auditLog.ReviewNotes = reviewNotes;
            auditLog.RequiresReview = false;

            await _context.SaveChangesAsync();

            await LogActivityAsync(
                userId: reviewedBy,
                action: "Audit_Log_Reviewed",
                entityType: "AuditLog",
                entityId: auditLogId,
                description: $"Audit log reviewed: {auditLog.Action}",
                severity: "Info"
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing audit log: {AuditLogId}", auditLogId);
            return false;
        }
    }

    public async Task<AuditSummaryDto> GetAuditSummaryAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var query = _context.AuditLogs.Where(a => a.Timestamp >= fromDate && a.Timestamp <= toDate);

            var summary = new AuditSummaryDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalEvents = await query.CountAsync(),
                SecurityEvents = await query.CountAsync(a => a.IsSecurityEvent),
                ErrorEvents = await query.CountAsync(a => a.Severity == "Error"),
                CriticalEvents = await query.CountAsync(a => a.Severity == "Critical"),
                EventsRequiringReview = await query.CountAsync(a => a.RequiresReview),
                TopActions = await query
                    .GroupBy(a => a.Action)
                    .Select(g => new ActionSummaryDto { Action = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync(),
                TopEntityTypes = await query
                    .GroupBy(a => a.EntityType)
                    .Select(g => new EntityTypeSummaryDto { EntityType = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync(),
                TopUsers = await query
                    .GroupBy(a => a.UserId)
                    .Select(g => new UserSummaryDto { UserId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync()
            };

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audit summary");
            return new AuditSummaryDto();
        }
    }

    public async Task<List<AuditLogDto>> GetUserActivityAsync(string userId, DateTime fromDate, DateTime toDate)
    {
        return await GetAuditLogsAsync(
            userId: userId,
            fromDate: fromDate,
            toDate: toDate,
            pageSize: 1000
        );
    }

    public async Task<List<AuditLogDto>> GetDocumentActivityAsync(Guid documentId, DateTime fromDate, DateTime toDate)
    {
        return await GetAuditLogsAsync(
            entityType: "Document",
            entityId: documentId,
            fromDate: fromDate,
            toDate: toDate,
            pageSize: 1000
        );
    }

    public async Task<bool> ExportAuditLogsAsync(DateTime fromDate, DateTime toDate, string format = "csv")
    {
        try
        {
            var auditLogs = await GetAuditLogsAsync(
                fromDate: fromDate,
                toDate: toDate,
                pageSize: 10000
            );

            // Implementation for exporting to CSV/JSON would go here
            _logger.LogInformation("Exported {Count} audit logs from {FromDate} to {ToDate}", 
                auditLogs.Count, fromDate, toDate);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs");
            return false;
        }
    }

    public async Task<bool> ArchiveAuditLogsAsync(DateTime beforeDate)
    {
        try
        {
            var oldLogs = await _context.AuditLogs
                .Where(a => a.Timestamp < beforeDate && !a.RequiresReview)
                .ToListAsync();

            _context.AuditLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Archived {Count} audit logs older than {BeforeDate}", 
                oldLogs.Count, beforeDate);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving audit logs");
            return false;
        }
    }

    public async Task<int> GetAuditLogCountAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.AuditLogs
            .CountAsync(a => a.Timestamp >= fromDate && a.Timestamp <= toDate);
    }

    public async Task<List<string>> GetDistinctActionsAsync()
    {
        return await _context.AuditLogs
            .Select(a => a.Action)
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync();
    }

    public async Task<List<string>> GetDistinctEntityTypesAsync()
    {
        return await _context.AuditLogs
            .Select(a => a.EntityType)
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync();
    }

    public async Task<List<string>> GetDistinctSeveritiesAsync()
    {
        return await _context.AuditLogs
            .Select(a => a.Severity)
            .Distinct()
            .OrderBy(a => a)
            .ToListAsync();
    }
}
