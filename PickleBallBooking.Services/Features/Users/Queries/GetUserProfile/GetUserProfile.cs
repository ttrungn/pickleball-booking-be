using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Repositories.Repositories.Contexts;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Users;

namespace PickleBallBooking.Services.Users.Queries.GetUserProfile;

public record GetUserProfileQuery : IRequest<DataServiceResponse<UserResponse>>
{
    public string UserId { get; init; } = null!;
}

public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, DataServiceResponse<UserResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<DataServiceResponse<UserResponse>> Handle(
        GetUserProfileQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving user profile for UserId: {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                _logger.LogWarning("User not found with UserId: {UserId}", request.UserId);
                return new DataServiceResponse<UserResponse>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var userResponse = new UserResponse
            {
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            _logger.LogInformation("User profile retrieved successfully for UserId: {UserId}", request.UserId);

            return new DataServiceResponse<UserResponse>
            {
                Success = true,
                Message = "User profile retrieved successfully",
                Data = userResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user profile for UserId: {UserId}", request.UserId);
            return new DataServiceResponse<UserResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving user profile"
            };
        }
    }
}
