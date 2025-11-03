using System.Security.Claims;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleBallBooking.API.Mappers;
using PickleBallBooking.Services.Features.Bookings.Commands.CreateBooking;
using PickleBallBooking.Services.Features.Bookings.Commands.UpdateBooking;
using PickleBallBooking.Services.Features.Bookings.Queries.GetBookingById;
using PickleBallBooking.Services.Features.Bookings.Queries.GetBookingByUser;
using PickleBallBooking.Services.Features.Bookings.Queries.GetBookings;
using PickleBallBooking.Services.Models.Requests;

namespace PickleBallBooking.API.Controllers.Bookings.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/bookings")]
public class BookingController : ControllerBase
{
    private readonly ISender _sender;

    public BookingController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize]
    public async Task<IResult> CreateBookingAsync(
        [FromBody] CreateBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        command = command with { UserId = userId };
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return Results.Created($"/api/v1/bookings/{result.Data}", result.ToDataApiResponse());
        }

        return Results.BadRequest(result.ToDataApiResponse());
    }

    [HttpGet("users")]
    [Authorize]
    public async Task<IResult> GetMyBookingsAsync(
        [FromQuery] BookingGetRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        var query = new GetBookingByUserQuery
        {
            UserId = userId,
            FieldName = request.FieldName,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            Status = request.Status,
            IsActive = request.IsActive,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var result = await _sender.Send(query, cancellationToken);

        if (result.Success)
        {
            return Results.Ok(result.ToPaginatedApiResponse());
        }

        return Results.BadRequest(result.ToPaginatedApiResponse());
    }

    [HttpGet]
    public async Task<IResult> GetBookingsAsync(
        [FromQuery] BookingGetRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBookingsQuery
        {
            FieldName = request.FieldName,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            Status = request.Status,
            IsActive = request.IsActive,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var result = await _sender.Send(query, cancellationToken);

        if (result.Success)
        {
            return Results.Ok(result.ToPaginatedApiResponse());
        }

        return Results.BadRequest(result.ToPaginatedApiResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetBookingByIdAsync([FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBookingByIdQuery { Id = id };
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
        {
            return Results.Ok(result.ToDataApiResponse());
        }

        return Results.NotFound(result.ToDataApiResponse());
    }
    [HttpPut("{id:guid}/cancel")]
    public async Task<IResult> CancelBookingAsync([FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new CancelBookingCommand() { Id = id };
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
        {
            return Results.NoContent();
        }

        return Results.BadRequest(result.ToBaseApiResponse());
    }
}
