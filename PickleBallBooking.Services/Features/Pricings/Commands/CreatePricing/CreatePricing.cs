using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Services.Interfaces.Services;
using DayOfWeek = PickleBallBooking.Domain.Enums.DayOfWeek;

namespace PickleBallBooking.Services.Features.Pricings.Commands.CreatePricing;

public record CreatePricingCommand : IRequest<DataServiceResponse<List<Guid>>>
{
    public Guid FieldId { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public decimal Price { get; init; }
}

public class CreatePricingCommandValidator : AbstractValidator<CreatePricingCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private static readonly int[] AllowedMinutes = { 0, 30 };

    public CreatePricingCommandValidator(IUnitOfWork unitOfWork)
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


            var intervals = new List<(TimeOnly s, TimeOnly e)>();
            var cursor = cmd.StartTime;
            while (cursor < cmd.EndTime)
            {
                var next = cursor.AddMinutes(30);
                intervals.Add((cursor, next));
                cursor = next;
            }
            var intervalSet = intervals.ToHashSet();


            var candidates = await _unitOfWork.GetRepository<Pricing>().Query()
                .Include(p => p.TimeSlot)
                .Where(p => p.IsActive &&
                            p.FieldId == cmd.FieldId &&
                            p.DayOfWeek == cmd.DayOfWeek &&
                            p.TimeSlot.StartTime >= cmd.StartTime &&
                            p.TimeSlot.EndTime <= cmd.EndTime)
                .Select(p => new { p.TimeSlot.StartTime, p.TimeSlot.EndTime })
                .ToListAsync(ct);

            var duplicatePairs = candidates
                .Where(c => intervalSet.Contains((c.StartTime, c.EndTime)))
                .Select(c => $"{c.StartTime:HH:mm}-{c.EndTime:HH:mm}")
                .Distinct()
                .ToList();

            if (duplicatePairs.Any())
            {
                ctx.AddFailure($"Pricing already exists for slots: {string.Join(", ", duplicatePairs)}");
            }
        });
    }
}

public class CreatePricingCommandHandler : IRequestHandler<CreatePricingCommand, DataServiceResponse<List<Guid>>>
{
    private readonly ILogger<CreatePricingCommandHandler> _logger;
    private readonly IPricingService _pricingService;

    public CreatePricingCommandHandler(ILogger<CreatePricingCommandHandler> logger, IPricingService pricingService)
    {
        _logger = logger;
        _pricingService = pricingService;
    }

    public async Task<DataServiceResponse<List<Guid>>> Handle(CreatePricingCommand request, CancellationToken cancellationToken)
    {
        var response = await _pricingService.CreatePricingRangeAsync(request, cancellationToken);
        if (response.Success)
            _logger.LogInformation("Created {Count} pricing entries for Field {FieldId} DayOfWeek {DayOfWeek}", response.Data.Count, request.FieldId, request.DayOfWeek);
        else
            _logger.LogWarning("Create pricing range failed: {Message}", response.Message);
        return response;
    }
}
