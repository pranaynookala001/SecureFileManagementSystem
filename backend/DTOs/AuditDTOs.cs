namespace SecureDocumentAPI.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Details { get; set; }
    public string? Resource { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public bool IsSecurityEvent { get; set; }
    public bool RequiresReview { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewNotes { get; set; }
}

public class AuditSummaryDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalEvents { get; set; }
    public int SecurityEvents { get; set; }
    public int ErrorEvents { get; set; }
    public int CriticalEvents { get; set; }
    public int EventsRequiringReview { get; set; }
    public List<ActionSummaryDto> TopActions { get; set; } = new List<ActionSummaryDto>();
    public List<EntityTypeSummaryDto> TopEntityTypes { get; set; } = new List<EntityTypeSummaryDto>();
    public List<UserSummaryDto> TopUsers { get; set; } = new List<UserSummaryDto>();
}

public class ActionSummaryDto
{
    public string Action { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class EntityTypeSummaryDto
{
    public string EntityType { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class UserSummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public int Count { get; set; }
}
