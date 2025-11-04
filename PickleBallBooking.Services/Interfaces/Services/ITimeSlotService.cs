using PickleBallBooking.Services.Features.TimeSlots.Commands.CreateTimeSlot;
using PickleBallBooking.Services.Features.TimeSlots.Commands.DeleteTimeSlot;
using PickleBallBooking.Services.Features.TimeSlots.Commands.UpdateTimeSlot;
using PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlotById;
using PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlots;
using PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlotsByFieldAndDate;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Interfaces.Services;

public interface ITimeSlotService
{
    Task<DataServiceResponse<Guid>> CreateTimeSlotAsync(
        CreateTimeSlotCommand command,
        CancellationToken cancellationToken = default);

    Task<BaseServiceResponse> UpdateTimeSlotAsync(
        UpdateTimeSlotCommand command,
        CancellationToken cancellationToken = default);

    Task<BaseServiceResponse> DeleteTimeSlotAsync(
        DeleteTimeSlotCommand command,
        CancellationToken cancellationToken = default);

    Task<DataServiceResponse<TimeSlotResponse>> GetTimeSlotByIdAsync(
        GetTimeSlotByIdQuery query,
        CancellationToken cancellationToken = default);

    Task<PaginatedServiceResponse<TimeSlotResponse>> GetTimeSlotsAsync(
        GetTimeSlotsQuery query,
        CancellationToken cancellationToken = default);

    Task<DataServiceResponse<List<TimeSlotWithAvailabilityResponse>>> GetTimeSlotsByFieldAndDateAsync(
        GetTimeSlotsByFieldAndDateQuery query,
        CancellationToken cancellationToken = default);
}
