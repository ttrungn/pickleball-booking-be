using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Pricings.Queries.GetPricingById;

public record GetPricingByIdQuery : IRequest<DataServiceResponse<PricingResponse>>
{
    public Guid Id { get; init; }
}

public class GetPricingByIdQueryValidator : AbstractValidator<GetPricingByIdQuery>
{
    public GetPricingByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Pricing Id is required");
    }
}

public class GetPricingByIdQueryHandler : IRequestHandler<GetPricingByIdQuery, DataServiceResponse<PricingResponse>>
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<GetPricingByIdQueryHandler> _logger;

    public GetPricingByIdQueryHandler(IPricingService pricingService, ILogger<GetPricingByIdQueryHandler> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<PricingResponse>> Handle(GetPricingByIdQuery request, CancellationToken cancellationToken)
    {
        var response = await _pricingService.GetPricingByIdAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogWarning("Pricing {PricingId} not found: {Message}", request.Id, response.Message);
            return response;
        }
        _logger.LogInformation("Retrieved pricing {PricingId}", request.Id);
        return response;
    }
}
