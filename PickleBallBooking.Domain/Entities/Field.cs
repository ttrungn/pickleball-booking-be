namespace PickleBallBooking.Domain.Entities;


public class Field : BaseAuditableEntity<Guid>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public Guid FieldTypeId { get; set; }
    public decimal PricePerHour { get; set; }
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Area of the field (in square meters)
    /// </summary>
    public decimal? Area { get; set; }
    
    /// <summary>
    /// URL to the field blueprint image
    /// </summary>
    public string? BluePrintImageUrl { get; set; }
    
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Deeplink to Google Maps
    /// </summary>
    public string? MapUrl { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }

    // Navigation properties
    public FieldType FieldType { get; set; } = null!;
    public ICollection<FieldImage> Images { get; set; } = new List<FieldImage>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Pricing> Pricing { get; set; } = new List<Pricing>();
}

