using MediatR;
using PickleBallBooking.Services.Features.Auth.Commands.Login;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Auth.Commands.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, DataServiceResponse<TokenResponse>>
{
    private readonly ITokenService _tokenService;

    public RefreshTokenHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<DataServiceResponse<TokenResponse>> Handle(RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        return await _tokenService.RefreshTokens(request);
    }
}
