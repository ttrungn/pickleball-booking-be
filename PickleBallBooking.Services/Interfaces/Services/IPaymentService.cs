using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PickleBallBooking.Services.Models.Requests.Momo;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Momo;
using PickleBallBooking.Services.Momo.Commands.ConfirmMomoPayment;
using PickleBallBooking.Services.Momo.Queries.GetMomoPaymentUrl;

namespace PickleBallBooking.Services.Interfaces.Services;
public interface IPaymentService
{
    Task<DataServiceResponse<MomoCreatePaymentResponse?>> CreatePaymentWithMomo(GetMomoPaymentUrlQuery request);
    Task<BaseServiceResponse> ConfirmMomoPaymentAsync(ConfirmMomoPaymentCommand request);
}
