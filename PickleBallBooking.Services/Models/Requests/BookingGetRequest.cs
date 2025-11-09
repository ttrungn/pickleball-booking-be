using PickleBallBooking.Domain.Enums;

namespace PickleBallBooking.Services.Models.Requests;

public class BookingGetRequest
{
    public string? FieldName { get; init; } = string.Empty;
    public string? Email { get; init; } = string.Empty;
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public BookingStatus? Status { get; init; }
    public DateOnly? Date { get; init; }
    public bool? IsActive { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 8;
}
