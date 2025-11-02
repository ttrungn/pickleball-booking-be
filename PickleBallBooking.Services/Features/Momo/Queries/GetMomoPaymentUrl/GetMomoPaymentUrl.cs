using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Momo;

namespace PickleBallBooking.Services.Momo.Queries.GetMomoPaymentUrl;

public record GetMomoPaymentUrlQuery : IRequest<DataServiceResponse<MomoCreatePaymentResponse?>>
{
    public Guid BookingId { get; set; } = default!;
}

public class GetMomoPaymentUrlQueryValidator : AbstractValidator<GetMomoPaymentUrlQuery>
{
    public GetMomoPaymentUrlQueryValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty()
            .WithMessage("BookingId is required.");
    }
}

public class GetMomoPaymentUrlQueryHandler : IRequestHandler<GetMomoPaymentUrlQuery, DataServiceResponse<MomoCreatePaymentResponse?>>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<GetMomoPaymentUrlQueryHandler> _logger;

    public GetMomoPaymentUrlQueryHandler(IPaymentService paymentService, ILogger<GetMomoPaymentUrlQueryHandler> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<MomoCreatePaymentResponse?>> Handle(GetMomoPaymentUrlQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling MoMo payment URL request for BookingId: {BookingId}", request.BookingId);

        // Delegate to payment service (already builds MoMo payload and calls MoMo API)
        var result = await _paymentService.CreatePaymentWithMomo(request);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to get MoMo payment link for BookingId: {BookingId}. Message: {Message}", request.BookingId, result.Message);
            return new DataServiceResponse<MomoCreatePaymentResponse?>
            {
                Success = false,
                Message = result.Message,
                Data = result.Data
            };
        }

        _logger.LogInformation("Successfully obtained MoMo payment link for BookingId: {BookingId}", request.BookingId);
        return result;
    }
}
