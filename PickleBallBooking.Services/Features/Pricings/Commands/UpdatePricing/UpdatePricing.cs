using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Domain.Entities;
using DayOfWeek = PickleBallBooking.Domain.Enums.DayOfWeek;

namespace PickleBallBooking.Services.Features.Pricings.Commands.UpdatePricing;

public record UpdatePricingCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
    public Guid FieldId { get; init; }
    public Guid TimeSlotId { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    // Price removed - will be calculated automatically
}

public class UpdatePricingCommandValidator : AbstractValidator<UpdatePricingCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePricingCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Id).NotEmpty().WithMessage("Pricing ID is required!");
        RuleFor(x => x.FieldId).NotEmpty().WithMessage("Field ID is required!");
        RuleFor(x => x.TimeSlotId).NotEmpty().WithMessage("Time slot ID is required!");
        // Price validation removed - calculated automatically

        // Async validation: ensure pricing exists, referenced entities exist and no duplicate active pricing (excluding self)
        RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
        {
            // Ensure pricing exists and is active
            var pricingExists = await _unitOfWork.GetRepository<Pricing>().Query()
                .AnyAsync(p => p.Id == cmd.Id && p.IsActive, ct);
            if (!pricingExists)
            {
                ctx.AddFailure("Pricing not found!");
                return;
            }

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

            // Check duplicate pricing (same FieldId + TimeSlotId + DayOfWeek and active), excluding current record
            var duplicate = await _unitOfWork.GetRepository<Pricing>().Query()
                .AnyAsync(p => p.Id != cmd.Id && p.FieldId == cmd.FieldId && p.TimeSlotId == cmd.TimeSlotId && p.DayOfWeek == cmd.DayOfWeek && p.IsActive, ct);

            if (duplicate)
            {
                ctx.AddFailure("Another pricing with the same field/time/day already exists!");
            }
        });
    }
}

public class UpdatePricingCommandHandler : IRequestHandler<UpdatePricingCommand, BaseServiceResponse>
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<UpdatePricingCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePricingCommandHandler(IPricingService pricingService, ILogger<UpdatePricingCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _pricingService = pricingService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseServiceResponse> Handle(UpdatePricingCommand request, CancellationToken cancellationToken)
    {
        // Get existing pricing
        var pricing = await _unitOfWork.GetRepository<Pricing>().Query()
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.IsActive, cancellationToken);

        if (pricing == null)
        {
            return new BaseServiceResponse { Success = false, Message = "Pricing not found!" };
        }

        // Calculate price based on Field.PricePerHour and TimeSlot duration
        var field = await _unitOfWork.GetRepository<Field>().Query()
            .FirstOrDefaultAsync(f => f.Id == request.FieldId && f.IsActive, cancellationToken);
        
        var timeSlot = await _unitOfWork.GetRepository<TimeSlot>().Query()
            .FirstOrDefaultAsync(t => t.Id == request.TimeSlotId && t.IsActive, cancellationToken);

        if (field == null || timeSlot == null)
        {
            return new BaseServiceResponse 
            { 
                Success = false, 
                Message = "Field or TimeSlot not found!"
            };
        }

        // Calculate duration in hours
        var startTime = timeSlot.StartTime.ToTimeSpan();
        var endTime = timeSlot.EndTime.ToTimeSpan();
        var durationHours = (decimal)(endTime - startTime).TotalHours;

        // Calculate price automatically
        var calculatedPrice = field.PricePerHour * durationHours;

        // Update pricing fields
        pricing.FieldId = request.FieldId;
        pricing.TimeSlotId = request.TimeSlotId;
        pricing.DayOfWeek = request.DayOfWeek;
        pricing.Price = calculatedPrice;

        await _unitOfWork.GetRepository<Pricing>().UpdateAsync(pricing);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Pricing updated successfully with ID: {PricingId}, calculated price: {Price}", request.Id, calculatedPrice);
        return new BaseServiceResponse { Success = true, Message = "Pricing updated successfully!" };
    }
}
