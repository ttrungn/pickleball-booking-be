using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Users;
using PickleBallBooking.Services.Users.Commands.UpdateUserProfile;
using PickleBallBooking.Services.Users.Queries.GetAllUsers;

namespace PickleBallBooking.Services.Interfaces.Services;

public interface IUserService
{
    Task<BaseServiceResponse> UpdateUserProfileAsync(UpdateUserProfileCommand command, CancellationToken token = default);
    Task<PaginatedServiceResponse<GetUserResponse>> GetAllUsersAsync(GetAllUsersQuery query, CancellationToken token = default);
}
