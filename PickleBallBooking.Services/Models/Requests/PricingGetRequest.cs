namespace PickleBallBooking.Services.Models.Requests;

using PickleBallBooking.Domain.Enums;

public class PricingGetRequest
{
    public Guid? FieldId { get; set; }
    public Guid? TimeSlotId { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public bool? IsActive { get; set; } = true;
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 8;
}
