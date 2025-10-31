using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Domain.Enums;
using PickleBallBooking.Repositories.Extensions;

namespace PickleBallBooking.Repositories.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.ConfigureAuditableEntity();

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FieldId)
            .IsRequired();

        builder.Property(x => x.TimeSlotId)
            .IsRequired();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasDefaultValue(BookingStatus.Pending);

        builder.Property(x => x.TotalPrice)
            .HasPrecision(10, 2)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Field)
            .WithMany(f => f.Bookings)
            .HasForeignKey(x => x.FieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TimeSlot)
            .WithMany(t => t.Bookings)
            .HasForeignKey(x => x.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Payment)
            .WithMany(p => p.Bookings)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

