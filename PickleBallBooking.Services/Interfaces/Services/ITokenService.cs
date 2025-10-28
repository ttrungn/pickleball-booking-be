using PickleBallBooking.Services.Features.Auth.Commands.Login;
using PickleBallBooking.Services.Features.Auth.Commands.RefreshToken;
using PickleBallBooking.Services.Features.Auth.Commands.RevokeRefreshToken;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Interfaces.Services;

public interface ITokenService
{
    Task<DataServiceResponse<TokenResponse>> GenerateTokens(string userId);
    Task<DataServiceResponse<TokenResponse>> RefreshTokens(RefreshTokenCommand request);
    Task<BaseServiceResponse> RevokeToken(RevokeRefreshTokenCommand command, string? revokedBy = null);
    Task<BaseServiceResponse> RevokeAllUserTokens(string userId, string? revokedBy = null);
    Task<DataServiceResponse<bool>> ValidateRefreshToken(string token);
    Task<BaseServiceResponse> CleanupExpiredTokens();
}
