namespace PickleBallBooking.Services.Models.Responses.Users;

/// <summary>
/// Response model for retrieving a single user
/// </summary>
public class GetUserResponse
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}
