using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Domain.Enums;

namespace PickleBallBooking.Repositories.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Amount)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.Method)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(x => x.TransactionCode)
            .HasMaxLength(100);

        builder.Property(x => x.PaidAt);

        // Relationships
        builder.HasMany(x => x.Bookings)
            .WithOne(b => b.Payment)
            .HasForeignKey(b => b.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.TransactionCode)
            .IsUnique();
    }
}
