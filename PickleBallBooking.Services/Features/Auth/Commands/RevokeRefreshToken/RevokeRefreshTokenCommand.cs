using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Auth.Commands.RevokeRefreshToken;

public record RevokeRefreshTokenCommand : IRequest<BaseServiceResponse>
{
    [Required] public string RefreshToken { get; set; } = null!;
}

public class RevokeRefreshTokenRequestValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
