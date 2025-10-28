using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Repositories.Repositories.Contexts;
using PickleBallBooking.Services.Exceptions;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, DataServiceResponse<TokenResponse>>
{
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<TokenResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _logger.LogInformation("Login attempt started for email={Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for email={Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded)
        {
            _logger.LogWarning("Login failed: invalid password for email={Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password");
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Login failed: unconfirmed email for email={Email}", request.Email);
            throw new UnauthorizedException("Email not confirmed");
        }

        _logger.LogInformation("Password verified successfully for email={Email}. Generating tokens...", request.Email);
        var tokenResponse = await _tokenService.GenerateTokens(user.Id);

        if (!tokenResponse.Success)
        {
            _logger.LogError("Token generation failed for userId={UserId}, email={Email}", user.Id, request.Email);
            throw new InvalidOperationException("Failed to generate tokens");
        }

        _logger.LogInformation("Login successful for email={Email}", request.Email);

        return new DataServiceResponse<TokenResponse>
        {
            Success = true, Message = "Login successful", Data = tokenResponse.Data
        };
    }
}
