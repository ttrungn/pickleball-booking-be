using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.TimeSlots.Commands.DeleteTimeSlot;

public record DeleteTimeSlotCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
}

public class DeleteTimeSlotCommandValidator : AbstractValidator<DeleteTimeSlotCommand>
{
    public DeleteTimeSlotCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Time slot ID is required");
    }
}

public class DeleteTimeSlotCommandHandler : IRequestHandler<DeleteTimeSlotCommand, BaseServiceResponse>
{
    private readonly ITimeSlotService _timeSlotService;
    private readonly ILogger<DeleteTimeSlotCommandHandler> _logger;

    public DeleteTimeSlotCommandHandler(ITimeSlotService timeSlotService, ILogger<DeleteTimeSlotCommandHandler> logger)
    {
        _timeSlotService = timeSlotService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(DeleteTimeSlotCommand request, CancellationToken cancellationToken)
    {
        var response = await _timeSlotService.DeleteTimeSlotAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to delete time slot: {Message}", response.Message);
            return response;
        }

        _logger.LogInformation("Time slot deleted: {Id}", request.Id);
        return response;
    }
}
