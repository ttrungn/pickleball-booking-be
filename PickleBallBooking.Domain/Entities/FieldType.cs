namespace PickleBallBooking.Domain.Entities;


public class FieldType : BaseAuditableEntity<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<Field> Fields { get; set; } = new List<Field>();
}
