namespace PickleBallBooking.Services.Models.Responses.Dashboards;

public class GetRevenueResponse
{
    public string View { get; set; } = "month"; // "day", "month", "year"
    public List<RevenueDataPoint> Data { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class RevenueDataPoint
{
    public string Label { get; set; } = null!; // e.g., "Jan 2025", "Week 1", "Monday"
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}
