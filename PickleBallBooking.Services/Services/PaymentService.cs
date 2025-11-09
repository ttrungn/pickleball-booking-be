using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Domain.Enums;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Mappers;
using PickleBallBooking.Services.Models.Configurations;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Momo;
using PickleBallBooking.Services.Momo.Commands.ConfirmMomoPayment;
using PickleBallBooking.Services.Momo.Queries.GetMomoPaymentUrl;

namespace PickleBallBooking.Services.Services;
public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;

    private readonly HttpClient _client;

    private readonly MomoSettings _momoSettings;

    private readonly IUnitOfWork _unitOfWork;
    public PaymentService(ILogger<PaymentService> logger, HttpClient client, IOptions<MomoSettings> options, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _client = client;
        _momoSettings = options.Value;
        _unitOfWork = unitOfWork;
    }

    

    public async Task<DataServiceResponse<MomoCreatePaymentResponse?>> CreatePaymentWithMomo(GetMomoPaymentUrlQuery request)
    {
     
        var amount = await GetBookingTotalPrice(request.BookingId);

        var momoRequest = request.ToMomoOneTimePayment(_momoSettings, (long)amount);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var requestJson = JsonSerializer.Serialize<MomoOneTimePayment>(momoRequest, options);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _logger.LogInformation("POST to URL: {Url}", _momoSettings.PaymentUrl);

        var response = await _client.PostAsync(_momoSettings.PaymentUrl, content);

        var body = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("MoMo HTTP {Status}: {Body}", response.StatusCode, body);

        var responseJson = await response.Content.ReadAsStringAsync();
        var momoResponse = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(
             responseJson,
             new JsonSerializerOptions
             {
                 PropertyNameCaseInsensitive = true
             });
        _logger.LogInformation("MoMo Raw Response: {Response}", responseJson);
        _logger.LogInformation("MoMo Status Code: {StatusCode}", response.StatusCode);

        return new DataServiceResponse<MomoCreatePaymentResponse?>
        {
            Message = "Get momo payment link successfully",
            Success = true,
            Data = momoResponse!,
        };

    }

    public async Task<BaseServiceResponse> ConfirmMomoPaymentAsync(ConfirmMomoPaymentCommand request)
    {
    _logger.LogInformation("Confirming MoMo payment for PartnerCode={PartnerCode}, OrderId={OrderId}", request.PartnerCode, request.OrderId);

        // Signature must be valid; fail fast if it isn't
        if (!request.IsValidSignature(_momoSettings))
        {
            _logger.LogError("Invalid signature for MoMo confirm");
            return new BaseServiceResponse
            {
                Message = "Invalid MoMo signature",
                Success = false
            };
        }
        var paymentRepo = _unitOfWork.GetRepository<Payment>();

        var bookingRepo = _unitOfWork.GetRepository<Booking>();

        var booking = await bookingRepo.Query().FirstOrDefaultAsync(b => b.Id.ToString() == request.OrderId);

        if (booking == null)
        {
            _logger.LogError("booking not found for bookingId {bookingId}", request.OrderId);
            return new BaseServiceResponse
            {
                Message = "Error while confirm payment",
                Success = false
            };
        }
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount ?? 0,
            PaidAt = DateTime.UtcNow,
            Method = Domain.Enums.PaymentMethod.Momo,
            TransactionCode = request.TransId?.ToString(),
        };

        await paymentRepo.InsertAsync(payment);
        booking.Status = BookingStatus.Completed;
        booking.PaymentId = payment.Id;
        booking.UpdatedAt = DateTime.UtcNow;
        await bookingRepo.UpdateAsync(booking);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Payment {PaymentId} confirmed. Order {OrderId} status = {Status}",
               payment.Id, booking.Id, booking.Status);

        return new BaseServiceResponse
        {
            Message = "Confirm Payment Successfully",
            Success = true,
        };
    }

    private async Task<long> GetBookingTotalPrice(Guid bookingId)
    {
        var bookingRepo =  _unitOfWork.GetRepository<Booking>();
        var booking = await bookingRepo.Query()
            .Where(b => b.Id == bookingId).FirstOrDefaultAsync();
        if(booking == null)
        {
            _logger.LogError("Booking not found for bookingId {bookingId}", bookingId);
            return 0;
        }   
        
        if(booking.Status != BookingStatus.Pending)
        {
            _logger.LogError("Booking status is not pending for bookingId {bookingId}", bookingId);
            return 0;
        }
        
        if(booking.TotalPrice <= 0)
        {
            _logger.LogError("Booking total price is invalid for bookingId {bookingId}", bookingId);
            return 0;
        }

        return (long)booking.TotalPrice;


    }   
}
