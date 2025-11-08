namespace PickleBallBooking.Services.Models.Responses;

using PickleBallBooking.Domain.Enums;

public class PricingResponse
{
    public Guid Id { get; set; }
    public Guid FieldId { get; set; }
    public Guid TimeSlotId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public decimal Price { get; set; }
    public TimeOnly TimeSlotStartTime { get; set; }
    public TimeOnly TimeSlotEndTime { get; set; }
    // New: tổng khoảng (range) mà batch pricing này thuộc về
    public TimeOnly RangeStartTime { get; set; }
    public TimeOnly RangeEndTime { get; set; }
}
