using SecureDocumentAPI.DTOs;

namespace SecureDocumentAPI.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress);
    Task<bool> LogoutAsync(string userId, string sessionToken);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<bool> ValidateTokenAsync(string token);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<bool> EnableTwoFactorAsync(string userId, string secret);
    Task<bool> ValidateTwoFactorAsync(string userId, string code);
    Task<bool> DisableTwoFactorAsync(string userId);
    Task<UserDto> GetCurrentUserAsync(string userId);
    Task<bool> UpdateProfileAsync(string userId, UpdateProfileRequestDto request);
    Task<List<UserSessionDto>> GetUserSessionsAsync(string userId);
    Task<bool> RevokeSessionAsync(string userId, string sessionId);
    Task<bool> RevokeAllSessionsAsync(string userId);
    Task<bool> LockUserAsync(string userId, string reason);
    Task<bool> UnlockUserAsync(string userId);
    Task<bool> IsUserLockedAsync(string userId);
    Task<int> GetFailedLoginAttemptsAsync(string userId);
    Task<bool> ResetFailedLoginAttemptsAsync(string userId);
    Task<bool> IncrementFailedLoginAttemptsAsync(string userId);
}
