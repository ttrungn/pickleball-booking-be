using PickleBallBooking.API.Models.Responses;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.API.Mappers;

public static class BaseServiceResponseMapper
{
    public static BaseApiResponse ToBaseApiResponse(this BaseServiceResponse baseServiceResponse)
    {
        return new BaseApiResponse() { Success = baseServiceResponse.Success, Message = baseServiceResponse.Message };
    }
}
