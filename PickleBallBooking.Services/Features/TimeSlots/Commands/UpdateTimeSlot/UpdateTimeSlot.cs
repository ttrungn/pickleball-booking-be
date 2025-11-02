using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.TimeSlots.Commands.UpdateTimeSlot;

public record UpdateTimeSlotCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
}

public class UpdateTimeSlotCommandValidator : AbstractValidator<UpdateTimeSlotCommand>
{
    public UpdateTimeSlotCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Time slot ID is required");
        RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("Start time must be before end time");
    }
}

public class UpdateTimeSlotCommandHandler : IRequestHandler<UpdateTimeSlotCommand, BaseServiceResponse>
{
    private readonly ITimeSlotService _timeSlotService;
    private readonly ILogger<UpdateTimeSlotCommandHandler> _logger;

    public UpdateTimeSlotCommandHandler(ITimeSlotService timeSlotService, ILogger<UpdateTimeSlotCommandHandler> logger)
    {
        _timeSlotService = timeSlotService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(UpdateTimeSlotCommand request, CancellationToken cancellationToken)
    {
        var response = await _timeSlotService.UpdateTimeSlotAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to update time slot: {Message}", response.Message);
            return response;
        }

        _logger.LogInformation("Time slot updated: {Id}", request.Id);
        return response;
    }
}

