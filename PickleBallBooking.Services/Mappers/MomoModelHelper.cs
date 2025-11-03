using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using PickleBallBooking.Services.Models.Configurations;
using PickleBallBooking.Services.Models.Requests.Momo;
using PickleBallBooking.Services.Momo.Commands.ConfirmMomoPayment;
using PickleBallBooking.Services.Momo.Queries.GetMomoPaymentUrl;
using PickleBallBooking.Services.Utils;

namespace PickleBallBooking.Services.Mappers;
public static class MomoModelMapper
{
    public static MomoOneTimePayment ToMomoOneTimePayment(
     this GetMomoPaymentUrlQuery request,
     MomoSettings options, long Amount)
    {
        var orderId = request.BookingId.ToString();
        var requestId = Guid.NewGuid().ToString();

        // Serialize OrderDetails to base64 extraData
        string extraDataJson = JsonSerializer.Serialize<object>(new());
        string extraDataBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(extraDataJson));

        var momoModel = new MomoOneTimePayment
        {
            PartnerCode = options.PartnerCode,
            OrderId = orderId,
            RequestId = requestId,
            Amount = Amount,
            OrderInfo = $"Thanh Toán FPTU Stadium",
            RedirectUrl = options.RedirectUrl,
            IpnUrl = options.IpnUrl,
            RequestType = options.RequestType,
            ExtraData = extraDataBase64,
            Lang = "vi"
        };

        momoModel.MakeSignature(options.AccessKey ?? "", options.SecretKey ?? "");

        return momoModel;
    }
    public static string GenerateSignature(this ConfirmMomoPaymentCommand request, MomoSettings options)
    {
        // Build the raw signature string exactly as MoMo specifies, using invariant formatting for numerics
        string sAmount = request.Amount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        string sResponseTime = request.ResponseTime?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        string sResultCode = request.ResultCode?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        string sTransId = request.TransId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        string rawHash = "accessKey=" + (options.AccessKey ?? string.Empty) +
                         "&amount=" + sAmount +
                         "&extraData=" + (request.ExtraData ?? string.Empty) +
                         "&message=" + (request.Message ?? string.Empty) +
                         "&orderId=" + (request.OrderId ?? string.Empty) +
                         "&orderInfo=" + (request.OrderInfo ?? string.Empty) +
                         "&orderType=" + (request.OrderType ?? string.Empty) +
                         "&partnerCode=" + (request.PartnerCode ?? string.Empty) +
                         "&payType=" + (request.PayType ?? string.Empty) +
                         "&requestId=" + (request.RequestId ?? string.Empty) +
                         "&responseTime=" + sResponseTime +
                         "&resultCode=" + sResultCode +
                         "&transId=" + sTransId;

        return HashHelper.HmacSHA256(rawHash, options.SecretKey ?? "");
    }

    public static bool IsValidSignature(this ConfirmMomoPaymentCommand request, MomoSettings options)
    {
        var expected = request.GenerateSignature(options);
        return string.Equals(expected, request.Signature, StringComparison.OrdinalIgnoreCase);
    }
}
