namespace PickleBallBooking.Services.Models.Responses;

public class FieldResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public Guid FieldTypeId { get; set; }
    public decimal PricePerHour { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Area { get; set; }
    public string? BluePrintImageUrl { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? MapUrl { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }

    public FieldTypeResponse? FieldType { get; set; } = null!;
}
