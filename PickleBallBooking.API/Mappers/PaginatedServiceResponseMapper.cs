using PickleBallBooking.API.Models.Responses;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.API.Mappers;

public static class PaginatedServiceResponseMapper
{
    public static PaginatedApiResponse<T> ToPaginatedApiResponse<T>(this PaginatedServiceResponse<T> paginatedApiResponse,
        HttpRequest? request = null, HttpResponse? response = null)
    {
        return new PaginatedApiResponse<T>()
        {
            Success = paginatedApiResponse.Success,
            Message = paginatedApiResponse.Message,
            PageNumber = paginatedApiResponse.PageNumber,
            PageSize = paginatedApiResponse.PageSize,
            TotalCount = paginatedApiResponse.TotalCount,
            TotalPages = paginatedApiResponse.TotalPages,
            Data = paginatedApiResponse.Data
        };
    }
}
