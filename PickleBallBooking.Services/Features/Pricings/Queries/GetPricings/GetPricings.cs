using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using DayOfWeek = PickleBallBooking.Domain.Enums.DayOfWeek;

namespace PickleBallBooking.Services.Features.Pricings.Queries.GetPricings;

public record GetPricingsQuery : IRequest<PaginatedServiceResponse<PricingResponse>>
{
    public Guid? FieldId { get; init; }
    public Guid? TimeSlotId { get; init; }
    public DayOfWeek? DayOfWeek { get; init; }
    public bool IsActive { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 8;
}

public class GetPricingsQueryValidator : AbstractValidator<GetPricingsQuery>
{
    public GetPricingsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than zero");
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page size must be greater than zero");
    }
}

public class GetPricingsQueryHandler : IRequestHandler<GetPricingsQuery, PaginatedServiceResponse<PricingResponse>>
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<GetPricingsQueryHandler> _logger;

    public GetPricingsQueryHandler(IPricingService pricingService, ILogger<GetPricingsQueryHandler> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
    }

    public async Task<PaginatedServiceResponse<PricingResponse>> Handle(GetPricingsQuery request, CancellationToken cancellationToken)
    {
        var response = await _pricingService.GetPricingsAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to get pricings: {Message}", response.Message);
            return response;
        }

        _logger.LogInformation("Pricings retrieved successfully. Count: {Count}", response.Data.Count);
        return response;
    }
}
