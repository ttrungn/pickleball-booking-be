using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using DayOfWeek = PickleBallBooking.Domain.Enums.DayOfWeek;

namespace PickleBallBooking.Services.Features.Pricings.Commands.UpdatePricingRange;

public record UpdatePricingRangeCommand : IRequest<BaseServiceResponse>
{
    public Guid FieldId { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public decimal Price { get; init; }
}

public class UpdatePricingRangeCommandValidator : AbstractValidator<UpdatePricingRangeCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private static readonly int[] AllowedMinutes = { 0, 30 };

    public UpdatePricingRangeCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.FieldId).NotEmpty().WithMessage("Field ID is required!");
        RuleFor(x => x.StartTime).NotEmpty().WithMessage("Start time is required!");
        RuleFor(x => x.EndTime).NotEmpty().WithMessage("End time is required!")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be greater than start time!");
        RuleFor(x => x.Price).GreaterThan(0m).WithMessage("Price must be greater than 0");

        RuleFor(x => x.StartTime.Minute).Must(m => AllowedMinutes.Contains(m)).WithMessage("Start time minutes must be 00 or 30");
        RuleFor(x => x.EndTime.Minute).Must(m => AllowedMinutes.Contains(m)).WithMessage("End time minutes must be 00 or 30");

        RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
        {
            var fieldExists = await _unitOfWork.GetRepository<Field>().Query()
                .AnyAsync(f => f.Id == cmd.FieldId && f.IsActive, ct);
            if (!fieldExists)
            {
                ctx.AddFailure("Field not found!");
                return;
            }

            var totalMinutes = (int)(cmd.EndTime.ToTimeSpan() - cmd.StartTime.ToTimeSpan()).TotalMinutes;
            if (totalMinutes % 30 != 0)
            {
                ctx.AddFailure("Time range must be divisible by 30 minutes");
                return;
            }
        });
    }
}

public class UpdatePricingRangeCommandHandler : IRequestHandler<UpdatePricingRangeCommand, BaseServiceResponse>
{
    private readonly ILogger<UpdatePricingRangeCommandHandler> _logger;
    private readonly IPricingService _pricingService;

    public UpdatePricingRangeCommandHandler(ILogger<UpdatePricingRangeCommandHandler> logger, IPricingService pricingService)
    {
        _logger = logger;
        _pricingService = pricingService;
    }

    public async Task<BaseServiceResponse> Handle(UpdatePricingRangeCommand request, CancellationToken cancellationToken)
    {
        var response = await _pricingService.UpdatePricingRangeAsync(request, cancellationToken);
        if (response.Success)
            _logger.LogInformation("Pricing range updated Field {FieldId} DayOfWeek {DayOfWeek} {Start}-{End}", request.FieldId, request.DayOfWeek, request.StartTime, request.EndTime);
        else
            _logger.LogWarning("Update pricing range failed: {Message}", response.Message);
        return response;
    }
}
