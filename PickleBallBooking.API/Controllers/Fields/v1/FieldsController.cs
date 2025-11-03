using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PickleBallBooking.API.Mappers;
using PickleBallBooking.Services.Features.Fields.Commands.CreateField;
using PickleBallBooking.Services.Features.Fields.Commands.DeleteField;
using PickleBallBooking.Services.Features.Fields.Commands.UpdateField;
using PickleBallBooking.Services.Features.Fields.Queries.GetFieldById;
using PickleBallBooking.Services.Features.Fields.Queries.GetFields;
using PickleBallBooking.Services.Models.Requests.Fields;

namespace PickleBallBooking.API.Controllers.Fields.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/fields")]
public class FieldsController
{
    private readonly ISender _sender;

    public FieldsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IResult> CreateFieldAsync([FromForm] CreateFieldCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return Results.Created($"/api/fields/{result.Data}", result.ToDataApiResponse());
        }

        return Results.BadRequest(result.ToDataApiResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateFieldAsync(
        [FromRoute] Guid id,
        [FromForm] FieldUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateFieldCommand()
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            PricePerHour = request.PricePerHour,
            ImageUrl = request.ImageUrl,
            Area = request.Area,
            BluePrintImageUrl = request.BluePrintImageUrl,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            MapUrl = request.MapUrl,
            City = request.City,
            District = request.District,
            FieldTypeId = request.FieldTypeId
        };

        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return Results.NoContent();
        }

        return Results.BadRequest(result.ToBaseApiResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteFieldAsync([FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteFieldCommand { Id = id };
        var result = await _sender.Send(command, cancellationToken);
        if (result.Success)
        {
            return Results.NoContent();
        }

        return Results.NotFound(result.ToBaseApiResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetFieldByIdAsync([FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFieldByIdQuery { Id = id };
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
        {
            return Results.Ok(result.ToDataApiResponse());
        }

        return Results.NotFound(result.ToDataApiResponse());
    }

    [HttpGet]
    public async Task<IResult> GetFieldsAsync([FromQuery] FieldGetRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFieldsQuery()
        {
            Name = request.Name ?? string.Empty,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            IsActive = request.IsActive ?? true,
            PageNumber = request.PageNumber ?? 0,
            PageSize = request.PageSize ?? 8
        };
        var result = await _sender.Send(query, cancellationToken);
        if (result.Success)
        {
            return Results.Ok(result.ToPaginatedApiResponse());
        }

        return Results.BadRequest(result.ToPaginatedApiResponse());
    }
}
