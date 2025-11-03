using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Momo.Commands.ConfirmMomoPayment;

public record ConfirmMomoPaymentCommand : IRequest<BaseServiceResponse>
{
    public string? PartnerCode { get; init; }
    public string? OrderId { get; init; }

    public string? RequestId { get; init; }

    public long? Amount { get; init; }

    public string? OrderInfo { get; init; }

    public string? OrderType { get; init; }

    public long? TransId { get; init; }

    public int? ResultCode { get; init; }

    public string? Message { get; init; }

    public string? PayType { get; init; }

    public long? ResponseTime { get; init; }

    public string? ExtraData { get; init; }

    public string? Signature { get; init; }
}

public class ConfirmMomoPaymentCommandValidator : AbstractValidator<ConfirmMomoPaymentCommand>
{
    public ConfirmMomoPaymentCommandValidator()
    {
    }
}

public class ConfirmMomoPaymentCommandHandler : IRequestHandler<ConfirmMomoPaymentCommand, BaseServiceResponse>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ConfirmMomoPaymentCommandHandler> _logger;

    public ConfirmMomoPaymentCommandHandler(IPaymentService paymentService, ILogger<ConfirmMomoPaymentCommandHandler> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(ConfirmMomoPaymentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received MoMo payment confirmation for OrderId: {OrderId}, TransId: {TransId}", request.OrderId, request.TransId);

        // Delegate confirmation logic to PaymentService
        var result = await _paymentService.ConfirmMomoPaymentAsync(request);

        if (!result.Success)
        {
            _logger.LogWarning("MoMo payment confirmation failed for OrderId: {OrderId}. Message: {Message}", request.OrderId, result.Message);
            return result;
        }

        _logger.LogInformation("MoMo payment confirmation succeeded for OrderId: {OrderId}", request.OrderId);
        return result;
    }
}
