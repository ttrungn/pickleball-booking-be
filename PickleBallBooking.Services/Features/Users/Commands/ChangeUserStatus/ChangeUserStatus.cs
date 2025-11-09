using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Users.Commands.ChangeUserStatus;

public record ChangeUserStatusCommand : IRequest<BaseServiceResponse>
{
    public string UserId { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}

public class ChangeUserStatusCommandHandler : IRequestHandler<ChangeUserStatusCommand, BaseServiceResponse>
{
    private readonly IUserService _userService;
    private readonly ILogger<ChangeUserStatusCommandHandler> _logger;

    public ChangeUserStatusCommandHandler(
        IUserService userService,
        ILogger<ChangeUserStatusCommandHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ChangeUserStatus command for UserId={UserId}", request.UserId);

        var result = await _userService.ChangeUserStatusAsync(request.UserId, request.IsActive, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("ChangeUserStatus failed for UserId={UserId}: {Message}", request.UserId, result.Message);
        }
        else
        {
            _logger.LogInformation("ChangeUserStatus succeeded for UserId={UserId}", request.UserId);
        }

        return result;
    }
}
