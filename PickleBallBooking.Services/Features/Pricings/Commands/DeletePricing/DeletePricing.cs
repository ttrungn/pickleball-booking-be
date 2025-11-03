using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Pricings.Commands.DeletePricing;

public record DeletePricingCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
}

public class DeletePricingCommandValidator : AbstractValidator<DeletePricingCommand>
{
    public DeletePricingCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Pricing ID is required!");
    }
}

public class DeletePricingCommandHandler : IRequestHandler<DeletePricingCommand, BaseServiceResponse>
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<DeletePricingCommandHandler> _logger;

    public DeletePricingCommandHandler(IPricingService pricingService, ILogger<DeletePricingCommandHandler> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(DeletePricingCommand request, CancellationToken cancellationToken)
    {
        var response = await _pricingService.DeletePricingAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to delete pricing: {Message}", response.Message);
            return response;
        }

        _logger.LogInformation("Pricing deleted successfully with ID: {PricingId}", request.Id);
        return response;
    }
}
