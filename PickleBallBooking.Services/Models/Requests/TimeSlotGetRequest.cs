namespace PickleBallBooking.Services.Models.Requests;

public class TimeSlotGetRequest
{
    public bool? IsActive { get; set; } = true;
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 8;
}
