using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Bookings.Commands.UpdateBooking;

public record CancelBookingCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
}

public class CancelBookingValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Booking ID is required!");
    }
}

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, BaseServiceResponse>
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(IBookingService bookingService, ILogger<CancelBookingCommandHandler> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var response = await _bookingService.CancelBookingAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to cancel booking with ID {BookingId}: {Message}", request.Id, response.Message);
            return response;
        }
        _logger.LogInformation("Booking with ID {BookingId} cancelled successfully.", request.Id);
        return response;
    }
}
