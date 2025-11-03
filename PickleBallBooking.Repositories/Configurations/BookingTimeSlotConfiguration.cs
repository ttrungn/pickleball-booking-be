using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickleBallBooking.Domain.Entities;

namespace PickleBallBooking.Repositories.Configurations;

public class BookingTimeSlotConfiguration : IEntityTypeConfiguration<BookingTimeSlot>
{
    public void Configure(EntityTypeBuilder<BookingTimeSlot> builder)
    {
        builder.ToTable("BookingTimeSlot");
        builder.Ignore(x => x.Id);
        builder.HasKey(x => new { x.BookingId, x.TimeSlotId });

        builder.Property(x => x.BookingId)
            .IsRequired();

        builder.Property(x => x.TimeSlotId)
            .IsRequired();

        builder.HasOne(x => x.Booking)
            .WithMany(b => b.BookingTimeSlots)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TimeSlot)
            .WithMany(t => t.BookingTimeSlots)
            .HasForeignKey(x => x.TimeSlotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
