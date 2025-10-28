using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PickleBallBooking.API.Mappers;
using PickleBallBooking.Services.Features.Auth.Commands.Login;
using PickleBallBooking.Services.Features.Auth.Commands.RefreshToken;
using PickleBallBooking.Services.Features.Auth.Commands.Register;
using PickleBallBooking.Services.Features.Auth.Commands.RevokeRefreshToken;

namespace PickleBallBooking.API.Controllers.Auth.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("customer/register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand registerCommand)
    {
        var result = await _sender.Send(registerCommand);
        return Ok(result.ToBaseApiResponse());
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand loginCommand)
    {
        var result = await _sender.Send(loginCommand);
        return Ok(result.ToDataApiResponse());
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand refreshTokenCommand)
    {
        var result = await _sender.Send(refreshTokenCommand);
        return Ok(result.ToDataApiResponse());
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeRefreshTokenCommand revokeRefreshTokenCommand)
    {
        var result = await _sender.Send(revokeRefreshTokenCommand);
        return Ok(result.ToBaseApiResponse());
    }
}
