namespace PickleBallBooking.Services.Models.Requests.Users;

/// <summary>
/// Request model for retrieving all users with filtering and pagination
/// </summary>
public class GetAllUsersRequest
{
    /// <summary>
    /// Search users by name (UserName, FirstName, or LastName)
    /// </summary>
    public string? SearchName { get; set; }

    /// <summary>
    /// Search users by email address
    /// </summary>
    public string? SearchEmail { get; set; }

    /// <summary>
    /// Filter users by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Page number for pagination (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of records per page (default: 10, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 10;
}
