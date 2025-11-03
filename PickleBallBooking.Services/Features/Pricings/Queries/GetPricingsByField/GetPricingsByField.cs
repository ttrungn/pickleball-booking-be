using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Pricings.Queries.GetPricingsByField;

public record GetPricingsByFieldQuery : IRequest<DataServiceResponse<List<PricingResponse>>>
{
    public Guid FieldId { get; init; }
}

public class GetPricingsByFieldQueryValidator : AbstractValidator<GetPricingsByFieldQuery>
{
    public GetPricingsByFieldQueryValidator()
    {
        RuleFor(x => x.FieldId)
            .NotEmpty().WithMessage("Field ID is required");
    }
}

public class GetPricingsByFieldQueryHandler : IRequestHandler<GetPricingsByFieldQuery, DataServiceResponse<List<PricingResponse>>>
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<GetPricingsByFieldQueryHandler> _logger;

    public GetPricingsByFieldQueryHandler(IPricingService pricingService, ILogger<GetPricingsByFieldQueryHandler> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<List<PricingResponse>>> Handle(GetPricingsByFieldQuery request, CancellationToken cancellationToken)
    {
        var response = await _pricingService.GetPricingsByFieldAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to get pricings for field {FieldId}: {Message}", request.FieldId, response.Message);
            return response;
        }

        _logger.LogInformation("Pricings retrieved successfully for field {FieldId}. Count: {Count}", request.FieldId, response.Data?.Count ?? 0);
        return response;
    }
}

