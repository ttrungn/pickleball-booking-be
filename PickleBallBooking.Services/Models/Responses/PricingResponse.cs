namespace PickleBallBooking.Services.Models.Responses;

using PickleBallBooking.Domain.Enums;

public class PricingResponse
{
    public Guid Id { get; set; }
    public Guid FieldId { get; set; }
    public Guid TimeSlotId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public decimal Price { get; set; }
}
