using PickleBallBooking.Services.Features.Pricings.Commands.CreatePricing;
using PickleBallBooking.Services.Features.Pricings.Commands.DeletePricing;
using PickleBallBooking.Services.Features.Pricings.Commands.UpdatePricing;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingById;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricings;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingsByField;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Interfaces.Services;

public interface IPricingService
{
    Task<DataServiceResponse<Guid>> CreatePricingAsync(
        CreatePricingCommand command,
        CancellationToken cancellationToken = default);

    Task<BaseServiceResponse> UpdatePricingAsync(
        UpdatePricingCommand command,
        CancellationToken cancellationToken = default);

    Task<BaseServiceResponse> DeletePricingAsync(
        DeletePricingCommand command,
        CancellationToken cancellationToken = default);

    Task<DataServiceResponse<PricingResponse>> GetPricingByIdAsync(
        GetPricingByIdQuery query,
        CancellationToken cancellationToken = default);

    Task<PaginatedServiceResponse<PricingResponse>> GetPricingsAsync(
        GetPricingsQuery query,
        CancellationToken cancellationToken = default);

    Task<DataServiceResponse<List<PricingResponse>>> GetPricingsByFieldAsync(
        GetPricingsByFieldQuery query,
        CancellationToken cancellationToken = default);
}
