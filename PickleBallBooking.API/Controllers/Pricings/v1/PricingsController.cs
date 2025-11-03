using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PickleBallBooking.API.Mappers;
using PickleBallBooking.Services.Features.Pricings.Commands.CreatePricing;
using PickleBallBooking.Services.Features.Pricings.Commands.DeletePricing;
using PickleBallBooking.Services.Features.Pricings.Commands.UpdatePricing;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingById;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricings;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingsByField;
using PickleBallBooking.Services.Models.Requests;

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
    public async Task<IResult> CreatePricingAsync(CreatePricingCommand command, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return Results.Created($"/api/pricings/{result.Data}", result.ToDataApiResponse());
        }

        return Results.BadRequest(result.ToDataApiResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdatePricingAsync([FromRoute] Guid id, [FromBody] UpdatePricingCommand request, CancellationToken cancellationToken = default)
    {
        var command = new UpdatePricingCommand
        {
            Id = id,
            FieldId= request.FieldId,
            TimeSlotId = request.TimeSlotId,
            DayOfWeek = request.DayOfWeek,
            //Price = request.Price
        };

        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return Results.NoContent();
        }

        return Results.BadRequest(result.ToBaseApiResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeletePricingAsync([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var command = new DeletePricingCommand { Id = id };
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return Results.NoContent();
        }

        return Results.NotFound(result.ToBaseApiResponse());
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

    [HttpGet("field/{fieldId:guid}")]
    public async Task<IResult> GetPricingsByFieldAsync([FromRoute] Guid fieldId, CancellationToken cancellationToken = default)
    {
        var query = new GetPricingsByFieldQuery { FieldId = fieldId };
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
        {
            return Results.Ok(result.ToDataApiResponse());
        }

        return Results.NotFound(result.ToDataApiResponse());
    }
}
