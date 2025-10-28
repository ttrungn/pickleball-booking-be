using FluentValidation;
using MediatR;
using PickleBallBooking.Services.Features.Auth.Commands.Login;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<DataServiceResponse<TokenResponse>>
{
    public string RefreshToken { get; set; } = null!;
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
