using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlots;

public record GetTimeSlotsQuery : IRequest<PaginatedServiceResponse<TimeSlotResponse>>
{
    public bool IsActive { get; init; } = true;
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 8;
}

public class GetTimeSlotsQueryValidator : AbstractValidator<GetTimeSlotsQuery>
{
    public GetTimeSlotsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than zero");
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page size must be greater than zero");
        RuleFor(x => x).Custom((q, ctx) =>
        {
            if (q.StartTime.HasValue && q.EndTime.HasValue && q.StartTime.Value >= q.EndTime.Value)
            {
                ctx.AddFailure("StartTime must be before EndTime");
            }
        });
    }
}

public class GetTimeSlotsQueryHandler : IRequestHandler<GetTimeSlotsQuery, PaginatedServiceResponse<TimeSlotResponse>>
{
    private readonly ITimeSlotService _timeSlotService;
    private readonly ILogger<GetTimeSlotsQueryHandler> _logger;

    public GetTimeSlotsQueryHandler(ITimeSlotService timeSlotService, ILogger<GetTimeSlotsQueryHandler> logger)
    {
        _timeSlotService = timeSlotService;
        _logger = logger;
    }

    public async Task<PaginatedServiceResponse<TimeSlotResponse>> Handle(GetTimeSlotsQuery request, CancellationToken cancellationToken)
    {
        var response = await _timeSlotService.GetTimeSlotsAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to get time slots: {Message}", response.Message);
            return response;
        }

        _logger.LogInformation("Time slots retrieved. Count: {Count}", response.Data?.Count ?? 0);
        return response;
    }
}
