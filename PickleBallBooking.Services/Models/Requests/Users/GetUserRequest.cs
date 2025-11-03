namespace PickleBallBooking.Services.Models.Requests.Users;

/// <summary>
/// Request model for retrieving a user by ID
/// </summary>
public class GetUserRequest
{
    /// <summary>
    /// The unique identifier of the user
    /// </summary>
    public string UserId { get; set; } = null!;
}
