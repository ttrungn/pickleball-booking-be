namespace PickleBallBooking.Services.Models.Responses;

public class PaginatedServiceResponse<T> : BaseServiceResponse
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public List<T> Data { get; set; } = [];
}
