using PickleBallBooking.Domain.Entities;

namespace PickleBallBooking.Domain.Interfaces.Entities;

public interface IApplicationUser
{
    string FirstName { get; set; }
    string LastName { get; set; }
    ICollection<RefreshToken> RefreshTokens { get; set; }
}
