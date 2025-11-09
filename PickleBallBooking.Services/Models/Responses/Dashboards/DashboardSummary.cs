namespace PickleBallBooking.Services.Dashboards.Queries.GetSummary;

public class DashboardSummary
{
    public int TotalBookings { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CompletedBookings { get; set; }
}