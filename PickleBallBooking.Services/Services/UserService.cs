using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    public UserService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
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
}
