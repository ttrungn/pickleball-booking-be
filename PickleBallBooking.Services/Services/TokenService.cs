using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Repositories.Repositories.Contexts;
using PickleBallBooking.Services.Exceptions;
using PickleBallBooking.Services.Features.Auth.Commands.Login;
using PickleBallBooking.Services.Features.Auth.Commands.RefreshToken;
using PickleBallBooking.Services.Features.Auth.Commands.RevokeRefreshToken;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Configurations;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<TokenService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        ILogger<TokenService> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<DataServiceResponse<TokenResponse>> GenerateTokens(string userId)
    {
        _logger.LogInformation("Generating tokens for userId={UserId}", userId);

        var user = await _userManager.FindByIdAsync(userId)
                   ?? throw new NotFoundException("User not found");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault()
                   ?? throw new InvalidOperationException("User has no role assigned");

        _logger.LogDebug("User {UserId} roles retrieved: {Roles}", user.Id, string.Join(",", roles));

        var accessToken = GenerateAccessToken(user, role);
        var refreshToken = await GenerateRefreshToken(user.Id);

        _logger.LogInformation("Tokens generated successfully for userId={UserId}", userId);

        var response = new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = _jwtSettings.AccessTokenExpirationMinutes * 60,
            RefreshTokenExpiresAt = _jwtSettings.RefreshTokenExpirationDays * 24 * 60 * 60
        };

        return new DataServiceResponse<TokenResponse>
        {
            Success = true, Message = "Tokens generated successfully", Data = response
        };
    }

    public async Task<DataServiceResponse<TokenResponse>> RefreshTokens(RefreshTokenCommand request)
    {
        _logger.LogInformation("Refreshing tokens using refreshToken={RefreshToken}", request.RefreshToken);

        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken>();
        var refreshToken = await refreshTokenRepository.Query()
                               .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken)
                           ?? throw new UnauthorizedException("Invalid or expired refresh token");

        if (!refreshToken.IsActive)
        {
            _logger.LogWarning("Refresh token is inactive or expired for token={RefreshToken}", request.RefreshToken);
            throw new UnauthorizedException("Invalid or expired refresh token");
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReasonRevoked = "Replaced by new token";

        var user = await _userManager.FindByIdAsync(refreshToken.ApplicationUserId)
                   ?? throw new NotFoundException("User not found");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault()
                   ?? throw new InvalidOperationException("User has no role assigned");

        _logger.LogDebug("Refreshing access token for userId={UserId}, role={Role}", user.Id, role);

        var newAccessToken = GenerateAccessToken(user, role);
        var newRefreshToken = await GenerateRefreshToken(refreshToken.ApplicationUserId);

        refreshToken.ReplacedByToken = newRefreshToken.Token;
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Tokens refreshed successfully for userId={UserId}", user.Id);

        var response = new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiresAt = _jwtSettings.AccessTokenExpirationMinutes * 60,
            RefreshTokenExpiresAt = _jwtSettings.RefreshTokenExpirationDays * 24 * 60 * 60
        };

        return new DataServiceResponse<TokenResponse>
        {
            Success = true, Message = "Tokens refreshed successfully", Data = response
        };
    }

    public async Task<BaseServiceResponse> RevokeToken(RevokeRefreshTokenCommand command, string? revokedBy = null)
    {
        _logger.LogInformation("Revoking refresh token={Token} by={RevokedBy}", command.RefreshToken, revokedBy);

        var token = command.RefreshToken;
        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken>();
        var refreshToken = await refreshTokenRepository.Query()
                               .FirstOrDefaultAsync(rt => rt.Token == token)
                           ?? throw new NotFoundException("Token not found");

        if (refreshToken.IsRevoked)
        {
            _logger.LogWarning("Attempted to revoke an already revoked token={Token}", token);
            throw new InvalidOperationException("Token already revoked");
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedBy = revokedBy;
        refreshToken.ReasonRevoked = "Manually revoked";

        refreshTokenRepository.Update(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Token revoked successfully: token={Token}", token);
        return new BaseServiceResponse { Success = true, Message = "Token revoked successfully" };
    }

    public async Task<BaseServiceResponse> RevokeAllUserTokens(string userId, string? revokedBy = null)
    {
        _logger.LogInformation("Revoking all active tokens for userId={UserId}", userId);

        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken>();
        var activeTokens = await refreshTokenRepository.Query()
            .Where(rt => rt.ApplicationUserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedBy = revokedBy;
            token.ReasonRevoked = "Bulk revocation";
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Revoked {Count} tokens for userId={UserId}", activeTokens.Count, userId);
        return new BaseServiceResponse { Success = true, Message = $"Revoked {activeTokens.Count} tokens for user" };
    }

    public async Task<DataServiceResponse<bool>> ValidateRefreshToken(string token)
    {
        _logger.LogDebug("Validating refresh token={Token}", token);

        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken>();
        var refreshToken = await refreshTokenRepository.Query()
                               .FirstOrDefaultAsync(rt => rt.Token == token)
                           ?? throw new UnauthorizedException("Invalid or expired refresh token");

        if (!refreshToken.IsActive)
        {
            _logger.LogWarning("Refresh token invalid or inactive: token={Token}", token);
            throw new UnauthorizedException("Invalid or expired refresh token");
        }

        _logger.LogInformation("Refresh token valid: token={Token}", token);
        return new DataServiceResponse<bool> { Success = true, Message = "Token is valid", Data = true };
    }

    public async Task<BaseServiceResponse> CleanupExpiredTokens()
    {
        _logger.LogInformation("Starting cleanup of expired tokens...");

        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken>();
        var expiredTokens = await refreshTokenRepository.Query()
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in expiredTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "Expired - cleaned up";
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} expired tokens", expiredTokens.Count);
        return new BaseServiceResponse { Success = true, Message = $"Cleaned up {expiredTokens.Count} expired tokens" };
    }

    private string GenerateAccessToken(ApplicationUser user, string role)
    {
        _logger.LogDebug("Generating access token for userId={UserId}, role={Role}", user.Id, role);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""), new Claim(ClaimTypes.Role, role),
            new Claim("firstName", user.FirstName), new Claim("lastName", user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        _logger.LogDebug("Access token created for userId={UserId}", user.Id);
        return jwt;
    }

    private async Task<RefreshToken> GenerateRefreshToken(string userId)
    {
        _logger.LogDebug("Generating refresh token for userId={UserId}", userId);

        var refreshTokenRepository = _unitOfWork.GetRepository<RefreshToken>();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = GenerateRandomToken(),
            ApplicationUserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await refreshTokenRepository.InsertAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Refresh token created successfully for userId={UserId}", userId);
        return refreshToken;
    }

    private static string GenerateRandomToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
