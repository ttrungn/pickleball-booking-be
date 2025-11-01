namespace PickleBallBooking.Domain.Entities;


public class TimeSlot : BaseAuditableEntity<Guid>
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    // Navigation properties
    public ICollection<BookingTimeSlot> BookingTimeSlots { get; set; } = new List<BookingTimeSlot>();
    public ICollection<Pricing> Pricing { get; set; } = new List<Pricing>();
}
