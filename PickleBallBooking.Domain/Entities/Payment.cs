namespace PickleBallBooking.Domain.Entities;

using PickleBallBooking.Domain.Enums;


public class Payment : BaseEntity<Guid>
{
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionCode { get; set; }
    public DateTime? PaidAt { get; set; }

    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

