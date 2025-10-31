using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Repositories.Extensions;

namespace PickleBallBooking.Repositories.Configurations;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.ToTable("TimeSlots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.EndTime)
            .IsRequired();

        builder.ConfigureAuditableEntity();

        // Relationships
        builder.HasMany(x => x.Bookings)
            .WithOne(b => b.TimeSlot)
            .HasForeignKey(b => b.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Pricing)
            .WithOne(p => p.TimeSlot)
            .HasForeignKey(p => p.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.StartTime, x.EndTime })
            .IsUnique();
    }
}
