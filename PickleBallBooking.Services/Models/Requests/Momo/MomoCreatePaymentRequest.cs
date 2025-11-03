using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickleBallBooking.Services.Models.Requests.Momo;
public class MomoCreatePaymentRequest
{
    public Guid OrderId { get; set; }
}
