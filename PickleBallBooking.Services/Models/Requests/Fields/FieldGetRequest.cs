namespace PickleBallBooking.Services.Models.Requests.Fields;

public class FieldGetRequest
{
    public string? Name { get; set; } = string.Empty;
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsActive { get; set; } = true;
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 8;
}
