using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Domain.Constants;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Repositories.Repositories.Contexts;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Users;
using PickleBallBooking.Services.Users.Commands.UpdateUserProfile;
using PickleBallBooking.Services.Users.Queries.GetAllUsers;

namespace PickleBallBooking.Services.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> UpdateUserProfileAsync(UpdateUserProfileCommand command, CancellationToken token = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId!);

        if (user is null)
        {
            return new BaseServiceResponse
            {
                Success = false,
                Message = "User not found"
            };
        }

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.PhoneNumber = command.PhoneNumber;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return new BaseServiceResponse
            {
                Success = true,
                Message = "User profile updated successfully"
            };
        }
        else
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return new BaseServiceResponse
            {
                Success = false,
                Message = $"Failed to update user profile: {errors}"
            };
        }
    }

    public async Task<PaginatedServiceResponse<GetUserResponse>> GetAllUsersAsync(GetAllUsersQuery query, CancellationToken token = default)
    {
        var customers = await _userManager.GetUsersInRoleAsync(Roles.Customer);
        var userQuery = customers.AsQueryable();

        // Search by name
        if (!string.IsNullOrWhiteSpace(query.SearchName))
        {
            userQuery = userQuery.Where(u =>
                (u.UserName != null && u.UserName.Contains(query.SearchName)) ||
                (u.FirstName != null && u.FirstName.Contains(query.SearchName)) ||
                (u.LastName != null && u.LastName.Contains(query.SearchName)));
        }

        // Search by email
        if (!string.IsNullOrWhiteSpace(query.SearchEmail))
        {
            userQuery = userQuery.Where(u => u.Email != null && u.Email.Contains(query.SearchEmail));
        }

        // Filter by IsActive status
        if (query.IsActive.HasValue)
        {
            userQuery = userQuery.Where(u => u.LockoutEnabled == !query.IsActive.Value);
        }

        // Get total count
        var totalCount = userQuery.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        // Apply pagination and ordering
        var users = userQuery
            .OrderByDescending(u => u.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var userResponses = users.Select(u => new GetUserResponse
        {
            Id = u.Id,
            UserName = u.UserName ?? string.Empty,
            Email = u.Email ?? string.Empty,
            FirstName = u.FirstName,
            LastName = u.LastName,
            PhoneNumber = u.PhoneNumber,
            IsActive = !u.LockoutEnabled,
        }).ToList();

        return new PaginatedServiceResponse<GetUserResponse>
        {
            Success = true,
            Message = "Customers retrieved successfully",
            Data = userResponses,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<BaseServiceResponse> ChangeUserStatusAsync(string userId, bool isActive, CancellationToken token = default)
    {
        try
        {
            _logger.LogInformation("Changing user status for UserId={UserId}, IsActive={IsActive}", userId, isActive);

            // Find the user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found with UserId={UserId}", userId);
                return new BaseServiceResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Check if user is a customer
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Roles.Customer))
            {
                _logger.LogWarning("Attempted to change status for non-customer user. UserId={UserId}, Roles={Roles}",
                    userId, string.Join(", ", roles));
                return new BaseServiceResponse
                {
                    Success = false,
                    Message = "Can only change status for customer accounts"
                };
            }

            // Update user status (LockoutEnabled = !IsActive means user is locked when not active)
            user.LockoutEnabled = !isActive;

            // If deactivating, set lockout end date to far future
            // If activating, clear lockout end date
            if (!isActive)
            {
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            else
            {
                user.LockoutEnd = null;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user status. UserId={UserId}, Errors={Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return new BaseServiceResponse
                {
                    Success = false,
                    Message = "Failed to update user status: " + string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            _logger.LogInformation("Successfully changed user status. UserId={UserId}, NewStatus={IsActive}",
                userId, isActive ? "Active" : "Inactive");

            return new BaseServiceResponse
            {
                Success = true,
                Message = $"User status successfully changed to {(isActive ? "active" : "inactive")}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing user status for UserId={UserId}", userId);
            return new BaseServiceResponse
            {
                Success = false,
                Message = "An error occurred while changing user status"
            };
        }
    }
}
