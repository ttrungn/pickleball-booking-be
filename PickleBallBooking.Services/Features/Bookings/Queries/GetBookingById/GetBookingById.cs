using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Bookings.Queries.GetBookingById;

public record GetBookingByIdQuery : IRequest<DataServiceResponse<BookingResponse>>
{
    public Guid Id { get; init; }
}

public class GetBookingByIdValidator : AbstractValidator<GetBookingByIdQuery>
{
    public GetBookingByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Booking ID is required!");
    }
}

public class GetBookingByIdHandler : IRequestHandler<GetBookingByIdQuery, DataServiceResponse<BookingResponse>>
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<GetBookingByIdHandler> _logger;

    public GetBookingByIdHandler(IBookingService bookingService, ILogger<GetBookingByIdHandler> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<BookingResponse>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var response = await _bookingService.GetBookingByIdAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to retrieve booking: {Message}", response.Message);
            return response;
        }
        _logger.LogInformation("Booking retrieved successfully with ID: {BookingId}", request.Id);
        return response;
    }
}
