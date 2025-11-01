namespace PickleBallBooking.Services.Models.Responses;

public class TimeSlotResponse
{
    public Guid Id { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

