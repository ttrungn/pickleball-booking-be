using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlotsByFieldAndDate;

public record GetTimeSlotsByFieldAndDateQuery : IRequest<DataServiceResponse<List<TimeSlotWithAvailabilityResponse>>>
{
    public Guid FieldId { get; init; }
    public DateOnly Date { get; init; }
}

public class GetTimeSlotsByFieldAndDateQueryValidator : AbstractValidator<GetTimeSlotsByFieldAndDateQuery>
{
    public GetTimeSlotsByFieldAndDateQueryValidator()
    {
        RuleFor(x => x.FieldId)
            .NotEmpty()
            .WithMessage("Field ID is required");
    }
}

public class GetTimeSlotsByFieldAndDateQueryHandler : IRequestHandler<GetTimeSlotsByFieldAndDateQuery, DataServiceResponse<List<TimeSlotWithAvailabilityResponse>>>
{
    private readonly ITimeSlotService _timeSlotService;
    private readonly ILogger<GetTimeSlotsByFieldAndDateQueryHandler> _logger;

    public GetTimeSlotsByFieldAndDateQueryHandler(
        ITimeSlotService timeSlotService,
        ILogger<GetTimeSlotsByFieldAndDateQueryHandler> logger)
    {
        _timeSlotService = timeSlotService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<List<TimeSlotWithAvailabilityResponse>>> Handle(
        GetTimeSlotsByFieldAndDateQuery request,
        CancellationToken cancellationToken)
    {
        var response = await _timeSlotService.GetTimeSlotsByFieldAndDateAsync(request, cancellationToken);
        
        if (!response.Success)
        {
            _logger.LogError("Failed to get time slots for field {FieldId} on date {Date}: {Message}",
                request.FieldId, request.Date, response.Message);
            return response;
        }

        _logger.LogInformation("Time slots retrieved for field {FieldId} on date {Date}. Count: {Count}",
            request.FieldId, request.Date, response.Data?.Count ?? 0);
        return response;
    }
}

