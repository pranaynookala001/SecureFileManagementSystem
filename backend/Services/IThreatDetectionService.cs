using SecureDocumentAPI.DTOs;

namespace SecureDocumentAPI.Services;

public interface IThreatDetectionService
{
    Task<string> CheckLoginThreatAsync(string ipAddress);
    Task<bool> DetectSuspiciousActivityAsync(string userId, string action, string resource);
    Task<bool> DetectDataExfiltrationAsync(string userId, string documentId, string action);
    Task<bool> DetectBruteForceAttemptAsync(string ipAddress, string username);
    Task<bool> DetectUnusualAccessPatternAsync(string userId, string resource);
    Task<bool> DetectAnomalousBehaviorAsync(string userId);
    Task<List<SecurityAlertDto>> GetSecurityAlertsAsync(DateTime fromDate, DateTime toDate);
    Task<bool> CreateSecurityAlertAsync(string alertType, string description, string severity, string userId);
    Task<bool> ResolveSecurityAlertAsync(Guid alertId, string resolvedBy, string resolutionNotes);
}
