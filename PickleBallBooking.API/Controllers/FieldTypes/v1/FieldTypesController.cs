using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PickleBallBooking.API.Mappers;
using PickleBallBooking.Services.Features.FieldTypes.Queries.GetFieldTypes;
using PickleBallBooking.Services.Models.Requests;

namespace PickleBallBooking.API.Controllers.FieldTypes.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/field-types")]
public class FieldTypesController
{
    private readonly ISender _sender;

    public FieldTypesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetFieldTypesAsync(
        [FromQuery] FieldTypeGetRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFieldTypesQuery()
        {
            Name = request.Name ?? string.Empty,
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
}
