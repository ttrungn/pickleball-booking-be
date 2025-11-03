using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PickleBallBooking.API.Mappers;
using PickleBallBooking.Services.Momo.Commands.ConfirmMomoPayment;
using PickleBallBooking.Services.Momo.Queries.GetMomoPaymentUrl;

namespace PickleBallBooking.API.Controllers.Payments.v1;

[ApiVersion(1)]
[ApiController]
[Route("api/v{v:apiVersion}/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("momo/url")]
    public async Task<IActionResult> GetMomoPaymentUrl([FromQuery]GetMomoPaymentUrlQuery query, CancellationToken token = default)
    {

        var response =  await _sender.Send(query, token);   
        if (!response.Success)
        {
            return BadRequest(response.ToDataApiResponse());
        }   

        // Placeholder for getting payments logic
        return Ok(response.ToDataApiResponse());
    }


    [HttpPost("momo")]
    public async Task<IActionResult> ConfirmMomoPayment([FromBody]ConfirmMomoPaymentCommand request, CancellationToken token = default)
    {
        var response = await _sender.Send(request, token);
        if (!response.Success)
        {
            return BadRequest(response.ToBaseApiResponse());
        }

        // Placeholder for creating payment logic
        return Ok(response.ToBaseApiResponse());
    }   
}
