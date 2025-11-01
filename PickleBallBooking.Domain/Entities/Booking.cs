namespace PickleBallBooking.Domain.Entities;

using PickleBallBooking.Domain.Enums;


public class Booking : BaseAuditableEntity<Guid>
{
    public string UserId { get; set; } = null!;
    public Guid FieldId { get; set; }
    public Guid TimeSlotId { get; set; }
    public Guid? PaymentId { get; set; }
    public DateOnly Date { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public decimal TotalPrice { get; set; }

    // Navigation properties
    public Field Field { get; set; } = null!;
    public ICollection<BookingTimeSlot> BookingTimeSlots { get; set; } = new List<BookingTimeSlot>();
    public Payment? Payment { get; set; }
}

