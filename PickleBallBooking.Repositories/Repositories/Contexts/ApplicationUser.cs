using Microsoft.AspNetCore.Identity;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Domain.Interfaces.Entities;

namespace PickleBallBooking.Repositories.Repositories.Contexts;

public class ApplicationUser : IdentityUser, IApplicationUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
