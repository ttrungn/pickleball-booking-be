namespace PickleBallBooking.Domain.Entities;

using PickleBallBooking.Domain.Enums;


public class Pricing : BaseAuditableEntity<Guid>
{
    public Guid FieldId { get; set; }
    public Guid TimeSlotId { get; set; }
    
  
    public DayOfWeek DayOfWeek { get; set; }
    
    public decimal Price { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    // Navigation properties
    public Field Field { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
}
