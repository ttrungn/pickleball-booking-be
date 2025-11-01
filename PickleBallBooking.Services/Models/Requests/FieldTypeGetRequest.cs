namespace PickleBallBooking.Services.Models.Requests;

public class FieldTypeGetRequest
{
    public string? Name { get; init; } = string.Empty;
    public bool? IsActive { get; init; } = true;
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 8;
}
