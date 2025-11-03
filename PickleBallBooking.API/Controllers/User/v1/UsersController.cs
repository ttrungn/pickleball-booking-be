using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PickleBallBooking.API.Mappers;
using PickleBallBooking.Domain.Constants;
using PickleBallBooking.Repositories.Repositories.Contexts;
using PickleBallBooking.Services.Users.Commands.UpdateUserProfile;
using PickleBallBooking.Services.Users.Queries.GetUserProfile;
using PickleBallBooking.Services.Users.Queries.GetAllUsers;

namespace PickleBallBooking.API.Controllers.User.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    private readonly UserManager<ApplicationUser> _userManager;
    public UsersController(ISender sender, UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
        _sender = sender;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile(CancellationToken cancellationToken = default)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) {
            return Unauthorized();
        }
        var query = new GetUserProfileQuery()
        {
            UserId = user.Id,
        };

        var response = await _sender.Send(query, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(new { message = response.Message });
        }

        return Ok(response);
    }
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }
        command.UserId = user.Id;
        var response = await _sender.Send(command, cancellationToken);
        return Ok(response.ToBaseApiResponse());
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] PickleBallBooking.Services.Models.Requests.Users.GetAllUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllUsersQuery
        {
            SearchName = request.SearchName,
            SearchEmail = request.SearchEmail,
            IsActive = request.IsActive,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var response = await _sender.Send(query, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

}
