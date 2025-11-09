using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Dashboards.Queries.GetSummary;

public record GetSummaryQuery : IRequest<DataServiceResponse<DashboardSummary>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetSummaryQueryValidator : AbstractValidator<GetSummaryQuery>
{
    public GetSummaryQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate ?? DateTime.UtcNow)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("StartDate must be before or equal to EndDate");
    }
}

public class GetSummaryQueryHandler : IRequestHandler<GetSummaryQuery, DataServiceResponse<DashboardSummary>>
{
    private readonly IDashboardService _service;
    private readonly ILogger<GetSummaryQueryHandler> _logger;

    public GetSummaryQueryHandler(IDashboardService service, ILogger<GetSummaryQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<DataServiceResponse<DashboardSummary>> Handle(GetSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving dashboard summary with StartDate={StartDate}, EndDate={EndDate}", 
                request.StartDate, request.EndDate);
            
            var result = await _service.GetSummaryAsync(request.StartDate, request.EndDate);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to retrieve dashboard summary: {Message}", result.Message);
                return result;
            }

            _logger.LogInformation("Dashboard summary retrieved successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving dashboard summary");
            return new DataServiceResponse<DashboardSummary>
            {
                Success = false,
                Message = "An error occurred while retrieving dashboard summary",
                Data = null!
            };
        }
    }
}
