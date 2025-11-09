using System.Threading;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleBallBooking.API.Mappers;
using PickleBallBooking.Domain.Constants;
using PickleBallBooking.Services.Dashboards.Queries.GetRevenue;
using PickleBallBooking.Services.Dashboards.Queries.GetSummary;
using PickleBallBooking.Services.Dashboards.Queries.GetTopFields;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PickleBallBooking.API.Controllers.Dashboards.v1;


[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/dashboards")]
[Authorize(Roles = Roles.Admin)]
public class DashboardsController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("summary")]
    public async Task<IResult> GetSummary([FromQuery] GetSummaryQuery query,    
        CancellationToken cancellationToken = default)
    {
        var result =  await _sender.Send(query, cancellationToken);
        return Results.Ok(result.ToDataApiResponse());
    }

    [HttpGet("revenue")]
    public async Task<IResult> GetRevenue([FromQuery]GetRevenueQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(query, cancellationToken);
        return Results.Ok(result.ToDataApiResponse());
    }
    
     [HttpGet("top-fields")]
    public async Task<IResult> GetTopFields([FromQuery] GetTopFieldsQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(query, cancellationToken);
        return Results.Ok(result.ToDataApiResponse());
    }
}
