using Microsoft.AspNetCore.Http;

namespace PickleBallBooking.Services.Models.Requests;

public class FieldUpdateRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public decimal PricePerHour { get; set; }
    public IFormFile? ImageUrl { get; set; }
    public decimal? Area { get; set; }
    public IFormFile? BluePrintImageUrl { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? MapUrl { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public Guid FieldTypeId { get; set; }
}
