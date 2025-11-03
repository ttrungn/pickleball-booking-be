using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Domain.Entities;
using DayOfWeek = PickleBallBooking.Domain.Enums.DayOfWeek;

namespace PickleBallBooking.Services.Features.Pricings.Commands.CreatePricing;

public record CreatePricingCommand : IRequest<DataServiceResponse<Guid>>
{
    public Guid FieldId { get; init; }
    public Guid TimeSlotId { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    // Price removed - will be calculated automatically
}

public class CreatePricingCommandValidator : AbstractValidator<CreatePricingCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePricingCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.FieldId).NotEmpty().WithMessage("Field ID is required!");
        RuleFor(x => x.TimeSlotId).NotEmpty().WithMessage("Time slot ID is required!");
        // Price validation removed - calculated automatically

        // Async validation: ensure referenced entities exist and there is no duplicate active pricing
        RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
        {
            // Check field exists and is active
            var fieldExists = await _unitOfWork.GetRepository<Field>().Query()
                .AnyAsync(f => f.Id == cmd.FieldId && f.IsActive, ct);
            if (!fieldExists)
            {
                ctx.AddFailure("Field not found!");
                return;
            }

            // Check timeslot exists and is active
            var timeSlotExists = await _unitOfWork.GetRepository<TimeSlot>().Query()
                .AnyAsync(t => t.Id == cmd.TimeSlotId && t.IsActive, ct);
            if (!timeSlotExists)
            {
                ctx.AddFailure("Time slot not found!");
                return;
            }

            // Check duplicate pricing (same FieldId + TimeSlotId + DayOfWeek and active)
            var pricingExists = await _unitOfWork.GetRepository<Pricing>().Query()
                .AnyAsync(p => p.FieldId == cmd.FieldId && p.TimeSlotId == cmd.TimeSlotId && p.DayOfWeek == cmd.DayOfWeek && p.IsActive, ct);

            if (pricingExists)
            {
                ctx.AddFailure("Pricing for this field/time/day already exists!");
            }
        });
    }
}

public class CreatePricingCommandHandler : IRequestHandler<CreatePricingCommand, DataServiceResponse<Guid>>
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<CreatePricingCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePricingCommandHandler(IPricingService pricingService, ILogger<CreatePricingCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _pricingService = pricingService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<DataServiceResponse<Guid>> Handle(CreatePricingCommand request, CancellationToken cancellationToken)
    {
        // Calculate price based on Field.PricePerHour and TimeSlot duration
        var field = await _unitOfWork.GetRepository<Field>().Query()
            .FirstOrDefaultAsync(f => f.Id == request.FieldId && f.IsActive, cancellationToken);
        
        var timeSlot = await _unitOfWork.GetRepository<TimeSlot>().Query()
            .FirstOrDefaultAsync(t => t.Id == request.TimeSlotId && t.IsActive, cancellationToken);

        if (field == null || timeSlot == null)
        {
            return new DataServiceResponse<Guid> 
            { 
                Success = false, 
                Message = "Field or TimeSlot not found!", 
                Data = Guid.Empty 
            };
        }

        // Calculate duration in hours
        var startTime = timeSlot.StartTime.ToTimeSpan();
        var endTime = timeSlot.EndTime.ToTimeSpan();
        var durationHours = (decimal)(endTime - startTime).TotalHours;

        // Calculate price automatically
        var calculatedPrice = field.PricePerHour * durationHours;

        // Create pricing entity directly (bypass service to set calculated price)
        var id = Guid.NewGuid();
        var pricing = new Pricing
        {
            Id = id,
            FieldId = request.FieldId,
            TimeSlotId = request.TimeSlotId,
            DayOfWeek = request.DayOfWeek,
            Price = calculatedPrice
        };

        await _unitOfWork.GetRepository<Pricing>().InsertAsync(pricing, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Pricing created successfully with ID: {PricingId}, calculated price: {Price}", id, calculatedPrice);
        return new DataServiceResponse<Guid> { Success = true, Message = "Pricing created successfully", Data = id };
    }
}
