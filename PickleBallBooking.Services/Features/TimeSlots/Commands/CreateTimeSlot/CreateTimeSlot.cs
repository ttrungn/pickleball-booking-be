using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.TimeSlots.Commands.CreateTimeSlot;

public record CreateTimeSlotCommand : IRequest<DataServiceResponse<Guid>>
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
}

public class CreateTimeSlotCommandValidator : AbstractValidator<CreateTimeSlotCommand>
{
    public CreateTimeSlotCommandValidator()
    {
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("Start time must be before end time");
    }
}

public class CreateTimeSlotCommandHandler : IRequestHandler<CreateTimeSlotCommand, DataServiceResponse<Guid>>
{
    private readonly ITimeSlotService _timeSlotService;
    private readonly ILogger<CreateTimeSlotCommandHandler> _logger;

    public CreateTimeSlotCommandHandler(ITimeSlotService timeSlotService, ILogger<CreateTimeSlotCommandHandler> logger)
    {
        _timeSlotService = timeSlotService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<Guid>> Handle(CreateTimeSlotCommand request, CancellationToken cancellationToken)
    {
        var response = await _timeSlotService.CreateTimeSlotAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to create time slot: {Message}", response.Message);
            return response;
        }

        _logger.LogInformation("Time slot created with ID: {Id}", response.Data);
        return response;
    }
}

