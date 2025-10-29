namespace PickleBallBooking.Domain.Entities;


public class FieldImage : BaseEntity<Guid>
{
    public Guid FieldId { get; set; }
    public string ImageUrl { get; set; } = null!;

    // Navigation properties
    public Field Field { get; set; } = null!;
}
