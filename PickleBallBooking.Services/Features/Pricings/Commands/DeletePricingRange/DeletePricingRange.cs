using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using DayOfWeek = PickleBallBooking.Domain.Enums.DayOfWeek;

namespace PickleBallBooking.Services.Features.Pricings.Commands.DeletePricingRange;

// Command nhận thông tin range để soft delete nhiều pricing một lúc
public record DeletePricingRangeCommand : IRequest<BaseServiceResponse>
{
    public Guid FieldId { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
}

public class DeletePricingRangeCommandValidator : AbstractValidator<DeletePricingRangeCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeletePricingRangeCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.FieldId).NotEmpty().WithMessage("Field ID is required!");
        RuleFor(x => x.StartTime).NotEmpty().WithMessage("Start time is required!");
        RuleFor(x => x.EndTime).NotEmpty().WithMessage("End time is required!")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be greater than start time!");

        RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
        {
            var fieldExists = await _unitOfWork.GetRepository<Field>().Query()
                .AnyAsync(f => f.Id == cmd.FieldId && f.IsActive, ct);
            if (!fieldExists)
            {
                ctx.AddFailure("Field not found!");
            }
        });
    }
}

public class DeletePricingRangeCommandHandler : IRequestHandler<DeletePricingRangeCommand, BaseServiceResponse>
{
    private readonly ILogger<DeletePricingRangeCommandHandler> _logger;
    private readonly IPricingService _pricingService;
    public DeletePricingRangeCommandHandler(IPricingService pricingService, ILogger<DeletePricingRangeCommandHandler> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(DeletePricingRangeCommand request, CancellationToken cancellationToken)
    {
        var response = await _pricingService.DeletePricingRangeAsync(request, cancellationToken);
        if (response.Success)
            _logger.LogInformation("Soft deleted pricing range Field {FieldId} DayOfWeek {DayOfWeek} {Start}-{End}", request.FieldId, request.DayOfWeek, request.StartTime, request.EndTime);
        else
            _logger.LogWarning("Delete pricing range failed: {Message}", response.Message);
        return response;
    }
}
