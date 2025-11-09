using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Dashboards;

namespace PickleBallBooking.Services.Dashboards.Queries.GetRevenue;

public record GetRevenueQuery : IRequest<DataServiceResponse<GetRevenueResponse>>
{
    public string View { get; set; } = "month"; 
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetRevenueQueryValidator : AbstractValidator<GetRevenueQuery>
{
    public GetRevenueQueryValidator()
    {
        RuleFor(x => x.View)
            .Must(v => new[] { "day", "month", "year" }.Contains(v.ToLower()))
            .WithMessage("View must be 'day', 'month', or 'year'");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate ?? DateTime.UtcNow)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("StartDate must be before or equal to EndDate");
    }
}

public class GetRevenueQueryHandler : IRequestHandler<GetRevenueQuery, DataServiceResponse<GetRevenueResponse>>
{
    private readonly IDashboardService _service;
    private readonly ILogger<GetRevenueQueryHandler> _logger;

    public GetRevenueQueryHandler(IDashboardService service, ILogger<GetRevenueQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<DataServiceResponse<GetRevenueResponse>> Handle(GetRevenueQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving revenue data with View={View}, StartDate={StartDate}, EndDate={EndDate}", 
                request.View, request.StartDate, request.EndDate);
            
            var result = await _service.GetRevenueAsync(request.View, request.StartDate, request.EndDate);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to retrieve revenue data: {Message}", result.Message);
                return result;
            }

            _logger.LogInformation("Successfully retrieved revenue data with {DataPointCount} data points, TotalRevenue={TotalRevenue}", 
                result.Data?.Data?.Count ?? 0, result.Data?.TotalRevenue ?? 0);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving revenue data with View={View}", request.View);
            return new DataServiceResponse<GetRevenueResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving revenue data",
                Data = null!
            };
        }
    }
}
