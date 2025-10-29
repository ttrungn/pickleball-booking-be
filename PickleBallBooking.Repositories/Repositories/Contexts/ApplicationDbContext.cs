using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PickleBallBooking.Domain.Entities;

namespace PickleBallBooking.Repositories.Repositories.Contexts;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<FieldType> FieldTypes => Set<FieldType>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Pricing> Pricings => Set<Pricing>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<FieldImage> FieldImages => Set<FieldImage>();
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
