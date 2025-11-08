using PickleBallBooking.Domain.Enums;
using DayOfWeek = PickleBallBooking.Domain.Enums.DayOfWeek;

namespace PickleBallBooking.Services.Models.Requests;

public class PricingUpdateRequest
{
    public Guid FieldId { get; set; }
    public Guid TimeSlotId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public decimal Price { get; set; }
}
