using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Dashboards;

namespace PickleBallBooking.Services.Dashboards.Queries.GetTopFields;

public record GetTopFieldsQuery : IRequest<DataServiceResponse<GetTopFieldsResponse>>
{
    public int TopCount { get; set; } = 5;
}

public class GetTopFieldsQueryValidator : AbstractValidator<GetTopFieldsQuery>
{
    public GetTopFieldsQueryValidator()
    {
        RuleFor(x => x.TopCount)
            .GreaterThan(0).WithMessage("TopCount must be greater than 0")
            .LessThanOrEqualTo(20).WithMessage("TopCount must not exceed 20");
    }
}

public class GetTopFieldsQueryHandler : IRequestHandler<GetTopFieldsQuery, DataServiceResponse<GetTopFieldsResponse>>
{
    private readonly IDashboardService _service;
    private readonly ILogger<GetTopFieldsQueryHandler> _logger;

    public GetTopFieldsQueryHandler(IDashboardService service, ILogger<GetTopFieldsQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<DataServiceResponse<GetTopFieldsResponse>> Handle(GetTopFieldsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving top {TopCount} fields", request.TopCount);
            
            var result = await _service.GetTopFieldsAsync(request.TopCount);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to retrieve top fields: {Message}", result.Message);
                return result;
            }

            _logger.LogInformation("Successfully retrieved {FieldCount} top fields", result.Data?.TopFields?.Count ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving top fields with TopCount={TopCount}", request.TopCount);
            return new DataServiceResponse<GetTopFieldsResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving top fields",
                Data = null!
            };
        }
    }
}
