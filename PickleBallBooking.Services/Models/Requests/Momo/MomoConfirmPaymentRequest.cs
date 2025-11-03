using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickleBallBooking.Services.Models.Requests.Momo;
public class MomoConfirmPaymentRequest
{
    public string? PartnerCode { get; init; }
    public string? OrderId { get; init; }

    public string? RequestId { get; init; }

    public long? Amount { get; init; }

    public string? OrderInfo { get; init; }

    public string? OrderType { get; init; }

    public long? TransId { get; init; }

    public int? resultCode { get; init; }

    public string? Message { get; init; }

    public string? PayType { get; init; }

    public long? ResponseTime { get; init; }

    public string? ExtraData { get; init; }

    public string? Signature { get; init; }
}
