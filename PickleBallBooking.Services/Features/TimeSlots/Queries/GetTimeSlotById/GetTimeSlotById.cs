using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlotById;

public record GetTimeSlotByIdQuery : IRequest<DataServiceResponse<TimeSlotResponse>>
{
    public Guid Id { get; init; }
}

public class GetTimeSlotByIdQueryValidator : AbstractValidator<GetTimeSlotByIdQuery>
{
    public GetTimeSlotByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Time slot ID is required");
    }
}

public class GetTimeSlotByIdQueryHandler : IRequestHandler<GetTimeSlotByIdQuery, DataServiceResponse<TimeSlotResponse>>
{
    private readonly ITimeSlotService _timeSlotService;
    private readonly ILogger<GetTimeSlotByIdQueryHandler> _logger;

    public GetTimeSlotByIdQueryHandler(ITimeSlotService timeSlotService, ILogger<GetTimeSlotByIdQueryHandler> logger)
    {
        _timeSlotService = timeSlotService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<TimeSlotResponse>> Handle(GetTimeSlotByIdQuery request, CancellationToken cancellationToken)
    {
        var response = await _timeSlotService.GetTimeSlotByIdAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to get time slot by id: {Message}", response.Message);
            return response;
        }

        _logger.LogInformation("Time slot retrieved: {Id}", request.Id);
        return response;
    }
}

