namespace PickleBallBooking.Services.Models.Responses.Dashboards;

public class GetTopFieldsResponse
{
    public List<TopFieldItem> TopFields { get; set; } = new();
}

public class TopFieldItem
{
    public Guid FieldId { get; set; }
    public string FieldName { get; set; } = null!;
    public int BookingCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
