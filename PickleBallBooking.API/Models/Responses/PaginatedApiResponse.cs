﻿namespace PickleBallBooking.API.Models.Responses;

public class PaginatedApiResponse<T> : BaseApiResponse
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public List<T> Data { get; set; } = [];
}
