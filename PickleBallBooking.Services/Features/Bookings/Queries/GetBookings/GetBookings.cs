using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Domain.Enums;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Bookings.Queries.GetBookings;

public record GetBookingsQuery : IRequest<PaginatedServiceResponse<BookingResponse>>
{
    public string? FieldName { get; init; } = string.Empty;
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public BookingStatus? Status { get; init; }
    public bool? IsActive { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 8;
}

public class GetBookingsQueryValidator : AbstractValidator<GetBookingsQuery>
{
    public GetBookingsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than zero!");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero!");

        RuleFor(x => x.MinPrice)
            .LessThanOrEqualTo(x => x.MaxPrice).WithMessage("Min price must be less than or equal to max price!");
    }
}


public class GetBookingsHandler : IRequestHandler<GetBookingsQuery, PaginatedServiceResponse<BookingResponse>>
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<GetBookingsHandler> _logger;

    public GetBookingsHandler(IBookingService bookingService, ILogger<GetBookingsHandler> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    public async Task<PaginatedServiceResponse<BookingResponse>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var response = await _bookingService.GetBookingsAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to retrieve bookings: {Message}", response.Message);
            return response;
        }
        _logger.LogInformation("Bookings retrieved successfully. Count: {Count}", response.Data.Count);
        return response;
    }
}
