using FluentValidation;
using MediatR;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Auth.Commands.Login;

public record LoginCommand : IRequest<DataServiceResponse<TokenResponse>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; } = false;
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
