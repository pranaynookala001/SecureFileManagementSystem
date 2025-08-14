using SecureDocumentAPI.DTOs;

namespace SecureDocumentAPI.Services;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(string userId);
    Task<List<UserDto>> GetAllUsersAsync(int page = 1, int pageSize = 20);
    Task<bool> CreateUserAsync(CreateUserRequestDto request);
    Task<bool> UpdateUserAsync(string userId, UpdateUserRequestDto request);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> ActivateUserAsync(string userId);
    Task<bool> DeactivateUserAsync(string userId);
    Task<bool> ChangeUserRoleAsync(string userId, string newRole);
    Task<List<UserDto>> GetUsersByRoleAsync(string role);
    Task<bool> ResetUserPasswordAsync(string userId, string newPassword);
    Task<bool> EnableTwoFactorAsync(string userId);
    Task<bool> DisableTwoFactorAsync(string userId);
    Task<bool> VerifyEmailAsync(string userId);
    Task<bool> SendPasswordResetEmailAsync(string email);
    Task<bool> ValidatePasswordResetTokenAsync(string token);
    Task<bool> ResetPasswordWithTokenAsync(string token, string newPassword);
    Task<UserStatisticsDto> GetUserStatisticsAsync();
    Task<bool> SeedInitialAdminUserAsync();
}
