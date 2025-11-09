using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Bookings.Commands.CreateBooking;

public record CreateBookingCommand : IRequest<DataServiceResponse<Guid>>
{
    public Guid FieldId { get; init; }
    public DateOnly Date { get; init; }
    public List<Guid> TimeSlotIds { get; init; } = new();
    public decimal TotalPrice { get; init; }
    [JsonIgnore]
    public string? UserId { get; init; }
}

public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.FieldId)
            .NotEmpty().WithMessage("Field ID is required!");
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Booking date is required!")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("Booking date must be today or in the future!");
        RuleFor(x => x.TimeSlotIds)
            .NotEmpty().WithMessage("At least one time slot must be selected!");
        RuleFor(x => x.TotalPrice)
            .GreaterThan(0).WithMessage("Total price must be greater than 0!");
    }
}

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, DataServiceResponse<Guid>>
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<CreateBookingHandler> _logger;
    
    public CreateBookingHandler(IBookingService bookingService, ILogger<CreateBookingHandler> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var response = await _bookingService.CreateBookingAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to create booking: {Message}", response.Message);
            return response;
        }
        _logger.LogInformation("Booking created successfully with ID: {BookingId}", response.Data);
        return response;
    }
}
