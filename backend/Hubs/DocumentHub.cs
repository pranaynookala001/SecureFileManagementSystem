using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SecureDocumentAPI.Services;
using System.Security.Claims;

namespace SecureDocumentAPI.Hubs;

[Authorize]
public class DocumentHub : Hub
{
    private readonly IAuditService _auditService;
    private readonly IUserService _userService;
    private readonly ILogger<DocumentHub> _logger;

    public DocumentHub(IAuditService auditService, IUserService userService, ILogger<DocumentHub> logger)
    {
        _auditService = auditService;
        _userService = userService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            // Log connection
            await _auditService.LogActivityAsync(
                userId: userId,
                action: "SignalR_Connected",
                entityType: "Connection",
                description: $"User {username} connected to SignalR hub",
                severity: "Info"
            );
            
            _logger.LogInformation("User {Username} connected to SignalR hub", username);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            // Log disconnection
            await _auditService.LogActivityAsync(
                userId: userId,
                action: "SignalR_Disconnected",
                entityType: "Connection",
                description: $"User {username} disconnected from SignalR hub",
                severity: "Info"
            );
            
            _logger.LogInformation("User {Username} disconnected from SignalR hub", username);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinDocumentGroup(string documentId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(documentId, out var docId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"document_{documentId}");
            
            await _auditService.LogActivityAsync(
                userId: userId,
                action: "Join_Document_Group",
                entityType: "Document",
                entityId: docId,
                description: $"User joined document group for real-time collaboration",
                severity: "Info"
            );
        }
    }

    public async Task LeaveDocumentGroup(string documentId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(documentId, out var docId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"document_{documentId}");
            
            await _auditService.LogActivityAsync(
                userId: userId,
                action: "Leave_Document_Group",
                entityType: "Document",
                entityId: docId,
                description: $"User left document group",
                severity: "Info"
            );
        }
    }

    public async Task SendDocumentComment(string documentId, string comment)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(documentId, out var docId))
        {
            var message = new
            {
                DocumentId = documentId,
                UserId = userId,
                Username = username,
                Comment = comment,
                Timestamp = DateTime.UtcNow
            };
            
            await Clients.Group($"document_{documentId}").SendAsync("ReceiveDocumentComment", message);
            
            await _auditService.LogActivityAsync(
                userId: userId,
                action: "Send_Comment",
                entityType: "Document",
                entityId: docId,
                description: $"User {username} sent a comment on document",
                severity: "Info"
            );
        }
    }

    public async Task SendDocumentEdit(string documentId, string editData)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(documentId, out var docId))
        {
            var message = new
            {
                DocumentId = documentId,
                UserId = userId,
                Username = username,
                EditData = editData,
                Timestamp = DateTime.UtcNow
            };
            
            await Clients.Group($"document_{documentId}").SendAsync("ReceiveDocumentEdit", message);
            
            await _auditService.LogActivityAsync(
                userId: userId,
                action: "Document_Edit",
                entityType: "Document",
                entityId: docId,
                description: $"User {username} made an edit to document",
                severity: "Info"
            );
        }
    }

    public async Task SendTypingIndicator(string documentId, bool isTyping)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(documentId, out var docId))
        {
            var message = new
            {
                DocumentId = documentId,
                UserId = userId,
                Username = username,
                IsTyping = isTyping,
                Timestamp = DateTime.UtcNow
            };
            
            await Clients.Group($"document_{documentId}").SendAsync("ReceiveTypingIndicator", message);
        }
    }

    public async Task SendDocumentAccess(string documentId, string accessType)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(documentId, out var docId))
        {
            var message = new
            {
                DocumentId = documentId,
                UserId = userId,
                Username = username,
                AccessType = accessType, // "view", "edit", "download"
                Timestamp = DateTime.UtcNow
            };
            
            await Clients.Group($"document_{documentId}").SendAsync("ReceiveDocumentAccess", message);
            
            await _auditService.LogActivityAsync(
                userId: userId,
                action: $"Document_{accessType.ToUpper()}",
                entityType: "Document",
                entityId: docId,
                description: $"User {username} accessed document for {accessType}",
                severity: "Info"
            );
        }
    }

    public async Task SendNotification(string userId, string notificationType, string message)
    {
        var currentUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Only allow sending notifications to yourself or if you're an admin
        if (currentUserId == userId || Context.User?.IsInRole("Admin") == true)
        {
            var notification = new
            {
                Type = notificationType,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        }
    }

    public async Task SendSecurityAlert(string alertType, string message, string severity = "Warning")
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            var alert = new
            {
                Type = alertType,
                Message = message,
                Severity = severity,
                UserId = userId,
                Username = username,
                Timestamp = DateTime.UtcNow
            };
            
            // Send to admins and the user
            await Clients.Group("admins").SendAsync("ReceiveSecurityAlert", alert);
            await Clients.Group($"user_{userId}").SendAsync("ReceiveSecurityAlert", alert);
            
            await _auditService.LogActivityAsync(
                userId: userId,
                action: "Security_Alert",
                entityType: "Security",
                description: $"Security alert: {alertType} - {message}",
                severity: severity
            );
        }
    }
}
