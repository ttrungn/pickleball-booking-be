using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PickleBallBooking.API.Mappers;
using PickleBallBooking.Services.Features.Pricings.Commands.CreatePricing;
using PickleBallBooking.Services.Features.Pricings.Commands.DeletePricingRange;
using PickleBallBooking.Services.Features.Pricings.Commands.UpdatePricingRange;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingById;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricings;

using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingsByField;
using PickleBallBooking.Services.Models.Requests;
using PickleBallBooking.Services.Exceptions;

namespace PickleBallBooking.API.Controllers.Pricings.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/pricings")]
public class PricingsController
{
    private readonly ISender _sender;

    public PricingsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IResult> CreatePricingAsync([FromBody] CreatePricingCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _sender.Send(command, cancellationToken);
            if (result.Success)
            {
                return Results.Created("/api/pricings", result.ToDataApiResponse());
            }

            return Results.BadRequest(result.ToDataApiResponse());
        }
        catch (ValidationException ex)
        {
            // Return 400 with validation message
            return Results.BadRequest(new { Success = false, Message = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IResult> UpdatePricingRangeAsync([FromBody] UpdatePricingRangeCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return Results.Ok(result.ToBaseApiResponse());
        }
        return Results.BadRequest(result.ToBaseApiResponse());
    }

    [HttpDelete]
    public async Task<IResult> DeletePricingRangeAsync([FromBody] DeletePricingRangeCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return Results.Ok(result.ToBaseApiResponse());
        }
        return Results.BadRequest(result.ToBaseApiResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetPricingByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var query = new GetPricingByIdQuery { Id = id };
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
        {
            return Results.Ok(result.ToDataApiResponse());
        }

        return Results.NotFound(result.ToDataApiResponse());
    }

    [HttpGet]
    public async Task<IResult> GetPricingsAsync([FromQuery] PricingGetRequest request, CancellationToken cancellationToken = default)
    {
        var query = new GetPricingsQuery
        {
            FieldId = request.FieldId,
            TimeSlotId = request.TimeSlotId,
            DayOfWeek = request.DayOfWeek,
            IsActive = request.IsActive ?? true,
            PageNumber = request.PageNumber ?? 1,
            PageSize = request.PageSize ?? 8
        };

        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
        {
            return Results.Ok(result.ToPaginatedApiResponse());
        }

        return Results.BadRequest(result.ToPaginatedApiResponse());
    }

    [HttpGet("by-field/{fieldId:guid}")]
    public async Task<IResult> GetPricingsByFieldAsync([FromRoute] Guid fieldId, CancellationToken cancellationToken = default)
    {
        var query = new GetPricingsByFieldQuery { FieldId = fieldId };
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
            return Results.Ok(result.ToDataApiResponse());
        return Results.BadRequest(result.ToDataApiResponse());
    }

    
}
