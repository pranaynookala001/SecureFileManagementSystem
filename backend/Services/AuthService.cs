using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureDocumentAPI.Data;
using SecureDocumentAPI.DTOs;
using SecureDocumentAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace SecureDocumentAPI.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IAuditService _auditService;
    private readonly IThreatDetectionService _threatDetectionService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IAuditService auditService,
        IThreatDetectionService threatDetectionService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _auditService = auditService;
        _threatDetectionService = threatDetectionService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent)
    {
        try
        {
            // Check for brute force attempts
            var threatLevel = await _threatDetectionService.CheckLoginThreatAsync(ipAddress);
            if (threatLevel == "High" || threatLevel == "Critical")
            {
                await _auditService.LogActivityAsync(
                    userId: "anonymous",
                    action: "Login_Blocked_Threat",
                    entityType: "Authentication",
                    description: $"Login blocked due to threat level: {threatLevel}",
                    severity: "Warning",
                    ipAddress: ipAddress
                );
                
                throw new UnauthorizedAccessException("Login temporarily blocked due to security concerns.");
            }

            // Find user by username or email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Username);

            if (user == null)
            {
                await LogFailedLoginAttemptAsync(request.Username, ipAddress, "User not found");
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            // Check if user is locked
            if (user.IsLocked())
            {
                await _auditService.LogActivityAsync(
                    userId: user.Id.ToString(),
                    action: "Login_Blocked_Locked",
                    entityType: "Authentication",
                    description: $"Login attempt for locked user: {user.Username}",
                    severity: "Warning",
                    ipAddress: ipAddress
                );
                
                throw new UnauthorizedAccessException("Account is locked. Please contact administrator.");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                await _auditService.LogActivityAsync(
                    userId: user.Id.ToString(),
                    action: "Login_Blocked_Inactive",
                    entityType: "Authentication",
                    description: $"Login attempt for inactive user: {user.Username}",
                    severity: "Warning",
                    ipAddress: ipAddress
                );
                
                throw new UnauthorizedAccessException("Account is inactive.");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await IncrementFailedLoginAttemptsAsync(user.Id.ToString());
                await LogFailedLoginAttemptAsync(user.Username, ipAddress, "Invalid password");
                
                // Lock account after 5 failed attempts
                if (user.FailedLoginAttempts >= 4)
                {
                    await LockUserAsync(user.Id.ToString(), "Too many failed login attempts");
                    throw new UnauthorizedAccessException("Account locked due to too many failed attempts.");
                }
                
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            // Reset failed login attempts on successful login
            await ResetFailedLoginAttemptsAsync(user.Id.ToString());

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Create user session
            var session = new UserSession
            {
                UserId = user.Id,
                SessionToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                SessionType = "Web"
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            // Log successful login
            await _auditService.LogActivityAsync(
                userId: user.Id.ToString(),
                action: "Login_Success",
                entityType: "Authentication",
                description: $"Successful login for user: {user.Username}",
                severity: "Info",
                ipAddress: ipAddress
            );

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    EmailVerified = user.EmailVerified,
                    TwoFactorEnabled = user.TwoFactorEnabled
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            throw;
        }
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress)
    {
        try
        {
            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new InvalidOperationException("Username already exists.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, BCrypt.Net.BCrypt.GenerateSalt(12));

            // Create user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = "Viewer", // Default role
                IsActive = true,
                EmailVerified = false,
                CreatedBy = "System",
                UpdatedBy = "System"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Log registration
            await _auditService.LogActivityAsync(
                userId: user.Id.ToString(),
                action: "User_Registered",
                entityType: "User",
                description: $"New user registered: {user.Username}",
                severity: "Info",
                ipAddress: ipAddress
            );

            // Generate JWT token for immediate login
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Create user session
            var session = new UserSession
            {
                UserId = user.Id,
                SessionToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IpAddress = ipAddress,
                SessionType = "Web"
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = 3600,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    EmailVerified = user.EmailVerified,
                    TwoFactorEnabled = user.TwoFactorEnabled
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Username}", request.Username);
            throw;
        }
    }

    public async Task<bool> LogoutAsync(string userId, string sessionToken)
    {
        try
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.UserId.ToString() == userId && s.SessionToken == sessionToken);

            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    userId: userId,
                    action: "Logout_Success",
                    entityType: "Authentication",
                    description: "User logged out successfully",
                    severity: "Info"
                );

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        try
        {
            var session = await _context.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionToken == refreshToken && s.IsActive);

            if (session == null || session.IsExpired())
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            var user = session.User;
            if (!user.IsActive || user.IsLocked())
            {
                throw new UnauthorizedAccessException("User account is not active.");
            }

            // Generate new tokens
            var newToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Update session
            session.SessionToken = newRefreshToken;
            session.ExpiresAt = DateTime.UtcNow.AddDays(30);
            session.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                userId: user.Id.ToString(),
                action: "Token_Refreshed",
                entityType: "Authentication",
                description: "JWT token refreshed successfully",
                severity: "Info",
                ipAddress: ipAddress
            );

            return new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    EmailVerified = user.EmailVerified,
                    TwoFactorEnabled = user.TwoFactorEnabled
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
    {
        try
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return false;
            }

            // Hash new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
            user.PasswordChangedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Username;

            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                userId: userId,
                action: "Password_Changed",
                entityType: "User",
                description: "Password changed successfully",
                severity: "Info"
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<UserDto> GetCurrentUserAsync(string userId)
    {
        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        if (user == null)
        {
            return null!;
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsActive = user.IsActive,
            EmailVerified = user.EmailVerified,
            TwoFactorEnabled = user.TwoFactorEnabled
        };
    }

    public async Task<bool> LockUserAsync(string userId, string reason)
    {
        try
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            user.LockedUntil = DateTime.UtcNow.AddHours(1); // Lock for 1 hour
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                userId: userId,
                action: "User_Locked",
                entityType: "User",
                description: $"User locked: {reason}",
                severity: "Warning"
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UnlockUserAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            user.LockedUntil = null;
            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                userId: userId,
                action: "User_Unlocked",
                entityType: "User",
                description: "User unlocked by administrator",
                severity: "Info"
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsUserLockedAsync(string userId)
    {
        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        return user?.IsLocked() ?? false;
    }

    public async Task<int> GetFailedLoginAttemptsAsync(string userId)
    {
        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        return user?.FailedLoginAttempts ?? 0;
    }

    public async Task<bool> ResetFailedLoginAttemptsAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting failed login attempts for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IncrementFailedLoginAttemptsAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            user.FailedLoginAttempts++;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing failed login attempts for user: {UserId}", userId);
            return false;
        }
    }

    // Helper methods
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]!);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("FirstName", user.FirstName ?? ""),
            new("LastName", user.LastName ?? ""),
            new("EmailVerified", user.EmailVerified.ToString()),
            new("TwoFactorEnabled", user.TwoFactorEnabled.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task LogFailedLoginAttemptAsync(string username, string ipAddress, string reason)
    {
        await _auditService.LogActivityAsync(
            userId: "anonymous",
            action: "Login_Failed",
            entityType: "Authentication",
            description: $"Failed login attempt for {username}: {reason}",
            severity: "Warning",
            ipAddress: ipAddress
        );
    }

    // Implementation of remaining interface methods
    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        // Implementation for password reset request
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        // Implementation for password reset
        return true;
    }

    public async Task<bool> EnableTwoFactorAsync(string userId, string secret)
    {
        // Implementation for enabling 2FA
        return true;
    }

    public async Task<bool> ValidateTwoFactorAsync(string userId, string code)
    {
        // Implementation for 2FA validation
        return true;
    }

    public async Task<bool> DisableTwoFactorAsync(string userId)
    {
        // Implementation for disabling 2FA
        return true;
    }

    public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileRequestDto request)
    {
        // Implementation for profile update
        return true;
    }

    public async Task<List<UserSessionDto>> GetUserSessionsAsync(string userId)
    {
        // Implementation for getting user sessions
        return new List<UserSessionDto>();
    }

    public async Task<bool> RevokeSessionAsync(string userId, string sessionId)
    {
        // Implementation for revoking a session
        return true;
    }

    public async Task<bool> RevokeAllSessionsAsync(string userId)
    {
        // Implementation for revoking all sessions
        return true;
    }
}
