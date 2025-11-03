using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Users.Commands.UpdateUserProfile;

public record UpdateUserProfileCommand : IRequest<BaseServiceResponse>
{
    [JsonIgnore]
    public string? UserId { get; set; }
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;    
}

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
       

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MinimumLength(3)
            .WithMessage("Name must be at least 3 characters long");

        RuleFor(x => x.LastName)
           .NotEmpty()
           .MinimumLength(3)
           .WithMessage("Name must be at least 3 characters long");

        RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\d{10}$").WithMessage("Phone number must be exactly 10 digits.");

    }
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, BaseServiceResponse>
{
    private readonly IUserService _service;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;
    public UpdateUserProfileCommandHandler(IUserService service, ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _service = service;
        _logger = logger;

    }

    public async Task<BaseServiceResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling UpdateUserProfileCommand for UserId: {UserId}", request.UserId);
        var response = await _service.UpdateUserProfileAsync(request, cancellationToken);
        if (response.Success)
        {
            _logger.LogInformation("User profile updated successfully for UserId: {UserId}", request.UserId);
        }
        else
        {
            _logger.LogWarning("Failed to update user profile for Name: {UserId}. Reason: {Reason}", request.UserId, response.Message);
        }
        return response;
    }
}
