using PickleBallBooking.Services.Features.Pricings.Commands.CreatePricing;
using PickleBallBooking.Services.Features.Pricings.Commands.UpdatePricingRange;
using PickleBallBooking.Services.Features.Pricings.Commands.DeletePricingRange;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingById;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricings;

using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingsByField;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Interfaces.Services;

public interface IPricingService
{
    Task<DataServiceResponse<PricingResponse>> GetPricingByIdAsync(
        GetPricingByIdQuery query,
        CancellationToken cancellationToken = default);

    Task<PaginatedServiceResponse<PricingResponse>> GetPricingsAsync(
        GetPricingsQuery query,
        CancellationToken cancellationToken = default);

    Task<DataServiceResponse<List<PricingResponse>>> GetPricingsByFieldAsync(
        GetPricingsByFieldQuery query,
        CancellationToken cancellationToken = default);

    Task<DataServiceResponse<List<Guid>>> CreatePricingRangeAsync(CreatePricingCommand command, CancellationToken cancellationToken = default);
    Task<BaseServiceResponse> UpdatePricingRangeAsync(UpdatePricingRangeCommand command, CancellationToken cancellationToken = default);
    Task<BaseServiceResponse> DeletePricingRangeAsync(DeletePricingRangeCommand command, CancellationToken cancellationToken = default);


}
