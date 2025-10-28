using MediatR;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Auth.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, BaseServiceResponse>
{
    private readonly ITokenService _tokenService;

    public RevokeRefreshTokenCommandHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<BaseServiceResponse> Handle(RevokeRefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        return await _tokenService.RevokeToken(command);
    }
}
