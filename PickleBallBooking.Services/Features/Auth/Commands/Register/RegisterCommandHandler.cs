using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Domain.Constants;
using PickleBallBooking.Repositories.Repositories.Contexts;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Auth.Commands.Register;

public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, BaseServiceResponse>
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<RegisterCommandHandler> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<RegisterCommandHandler> logger,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _logger = logger;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<BaseServiceResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Registration request received for email={Email}, role={Role}", request.Email,
            request.Role);

        // 1) Email already exists -> throw 400
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            _logger.LogWarning("Attempted to register an existing user with email={Email}", request.Email);
            throw new InvalidOperationException("User with this email already exists");
        }

        // 2) Normalize & validate role
        var normalizedRole = request.Role switch
        {
            var r when string.Equals(r, Roles.Admin, StringComparison.OrdinalIgnoreCase) => Roles.Admin,
            var r when string.Equals(r, Roles.Customer, StringComparison.OrdinalIgnoreCase) => Roles.Customer,
            _ => throw new ArgumentException($"Cannot register {request.Role} account with email: {request.Email}")
        };

        if (!await _roleManager.RoleExistsAsync(normalizedRole))
        {
            _logger.LogError("Role validation failed for email={Email}, role={Role}", request.Email, request.Role);
            throw new ArgumentException($"Role '{normalizedRole}' does not exist", nameof(request.Role));
        }

        _logger.LogDebug("Validated registration for email={Email} with role={Role}", request.Email, normalizedRole);

        // 3) Build user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true
        };

        // 4) Transaction: create user + assign role atomically
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            _logger.LogInformation("Creating new user in database for email={Email}", request.Email);
            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("User creation failed for email={Email}: {Errors}", request.Email, errors);
                throw new InvalidOperationException($"Cannot register account with email: {request.Email}");
            }

            _logger.LogInformation("User created successfully for email={Email}. Assigning role={Role}", request.Email,
                normalizedRole);
            var roleResult = await _userManager.AddToRoleAsync(user, normalizedRole);
            if (!roleResult.Succeeded)
            {
                var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to assign role={Role} to user={Email}: {Errors}", normalizedRole,
                    request.Email, roleErrors);
                throw new InvalidOperationException($"Cannot register account with email: {request.Email}");
            }

            await tx.CommitAsync(ct);
            _logger.LogInformation("Registration transaction committed successfully for email={Email}", request.Email);

            return new BaseServiceResponse { Success = true, Message = "Registration successful" };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Registration transaction rolled back for email={Email}", request.Email);
            throw new InvalidOperationException($"Cannot register account with email: {request.Email}");
        }
    }
}
