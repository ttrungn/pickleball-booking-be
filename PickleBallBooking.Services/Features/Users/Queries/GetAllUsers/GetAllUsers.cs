using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Users;

namespace PickleBallBooking.Services.Users.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<PaginatedServiceResponse<GetUserResponse>>
{
    public string? SearchName { get; init; }
    public string? SearchEmail { get; init; }
    public bool? IsActive { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    public GetAllUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100");
    }
}

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PaginatedServiceResponse<GetUserResponse>>
{
    private readonly IUserService _userService;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(
        IUserService userService,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<PaginatedServiceResponse<GetUserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all users with filters");

            var response = await _userService.GetAllUsersAsync(request, cancellationToken);

            _logger.LogInformation("Users retrieved successfully - Total: {TotalCount}", response.TotalCount);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving users");
            return new PaginatedServiceResponse<GetUserResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving users",
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = 0,
                TotalPages = 0
            };
        }
    }
}


