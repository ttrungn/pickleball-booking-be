using PickleBallBooking.Domain.Enums;

namespace PickleBallBooking.Services.Models.Responses;

public class BookingResponse
{
    public Guid Id { get; set; }
    public Guid FieldId { get; set; }
    public string FieldName { get; set; } = null!;
    public Guid? PaymentId { get; set; }
    public string UserId { get; set; } = null!;
    public string? Email { get; set; }
    public DateOnly Date { get; set; }
    public string? Status { get; set; }
    public decimal TotalPrice { get; set; }
    public List<TimeSlotResponse> TimeSlots { get; set; } = new();
}
