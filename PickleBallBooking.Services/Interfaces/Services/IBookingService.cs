using PickleBallBooking.Services.Features.Bookings.Commands.CreateBooking;
using PickleBallBooking.Services.Features.Bookings.Commands.UpdateBooking;
using PickleBallBooking.Services.Features.Bookings.Queries.GetBookingById;
using PickleBallBooking.Services.Features.Bookings.Queries.GetBookingByUser;
using PickleBallBooking.Services.Features.Bookings.Queries.GetBookings;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Interfaces.Services;

public interface IBookingService
{
    Task<DataServiceResponse<Guid>> CreateBookingAsync(
        CreateBookingCommand command,
        CancellationToken cancellationToken = default);

    Task<PaginatedServiceResponse<BookingResponse>> GetBookingByUserAsync(
        GetBookingByUserQuery query,
        CancellationToken cancellationToken = default);
    Task<PaginatedServiceResponse<BookingResponse>> GetBookingsAsync(
        GetBookingsQuery query,
        CancellationToken cancellationToken = default);
    Task<DataServiceResponse<BookingResponse>> GetBookingByIdAsync(
        GetBookingByIdQuery query,
        CancellationToken cancellationToken = default);
    Task<BaseServiceResponse> CancelBookingAsync(
        CancelBookingCommand command,
        CancellationToken cancellationToken = default);
        
}
