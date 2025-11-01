namespace PickleBallBooking.Services.Models.Responses;

public class FieldTypeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
