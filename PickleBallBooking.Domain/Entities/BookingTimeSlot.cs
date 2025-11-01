namespace PickleBallBooking.Domain.Entities;

public class BookingTimeSlot : BaseAuditableEntity<(Guid, Guid)>
{
    public Guid BookingId { get; set; }
    public Guid TimeSlotId { get; set; }
    
    public Booking Booking { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
}
