using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PickleBallBooking.API.Mappers;
using PickleBallBooking.Services.Features.TimeSlots.Commands.CreateTimeSlot;
using PickleBallBooking.Services.Features.TimeSlots.Commands.DeleteTimeSlot;
using PickleBallBooking.Services.Features.TimeSlots.Commands.UpdateTimeSlot;
using PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlotById;
using PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlots;
using PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlotsByFieldAndDate;
using PickleBallBooking.Services.Models.Requests;

namespace PickleBallBooking.API.Controllers.TimeSlots.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/timeslots")]
public class TimeSlotsController
{
    private readonly ISender _sender;

    public TimeSlotsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IResult> CreateTimeSlotAsync([FromBody] CreateTimeSlotCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return Results.Created($"/api/timeslots/{result.Data}", result.ToDataApiResponse());
        }

        return Results.BadRequest(result.ToDataApiResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateTimeSlotAsync([FromRoute] Guid id, [FromBody] UpdateTimeSlotCommand request, CancellationToken cancellationToken = default)
    {
        var command = new UpdateTimeSlotCommand
        {
            Id = id,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
            return Results.NoContent();

        return Results.BadRequest(result.ToBaseApiResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteTimeSlotAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteTimeSlotCommand { Id = id };
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
            return Results.NoContent();

        return Results.NotFound(result.ToBaseApiResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetTimeSlotByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetTimeSlotByIdQuery { Id = id };
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
            return Results.Ok(result.ToDataApiResponse());

        return Results.NotFound(result.ToDataApiResponse());
    }

    [HttpGet]
    public async Task<IResult> GetTimeSlotsAsync([FromQuery] TimeSlotGetRequest request, CancellationToken cancellationToken = default)
    {
        var query = new GetTimeSlotsQuery
        {
            IsActive = request.IsActive ?? true,
            PageNumber = request.PageNumber ?? 1,
            PageSize = request.PageSize ?? 8
        };

        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
            return Results.Ok(result.ToPaginatedApiResponse());

        return Results.BadRequest(result.ToPaginatedApiResponse());
    }

    [HttpGet("fields/{fieldId:guid}")]
    public async Task<IResult> GetTimeSlotsByFieldAndDateAsync(
        [FromRoute] Guid fieldId,
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTimeSlotsByFieldAndDateQuery
        {
            FieldId = fieldId,
            Date = date
        };

        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
            return Results.Ok(result.ToDataApiResponse());

        return Results.NotFound(result.ToDataApiResponse());
    }
}
