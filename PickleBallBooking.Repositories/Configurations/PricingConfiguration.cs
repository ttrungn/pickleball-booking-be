using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Domain.Enums;
using PickleBallBooking.Repositories.Extensions;

namespace PickleBallBooking.Repositories.Configurations;

public class PricingConfiguration : IEntityTypeConfiguration<Pricing>
{
    public void Configure(EntityTypeBuilder<Pricing> builder)
    {
        builder.ToTable("Pricing");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.FieldId)
            .IsRequired();

        builder.Property(x => x.TimeSlotId)
            .IsRequired();

        builder.Property(x => x.DayOfWeek)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(10, 2)
            .IsRequired();



        builder.ConfigureAuditableEntity();

        // Relationships
        builder.HasOne(x => x.Field)
            .WithMany(f => f.Pricing)
            .HasForeignKey(x => x.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TimeSlot)
            .WithMany(t => t.Pricing)
            .HasForeignKey(x => x.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite unique index
        builder.HasIndex(x => new { x.FieldId, x.TimeSlotId, x.DayOfWeek })
            .IsUnique();

        // Additional indexes
        builder.HasIndex(x => x.Price);
    }
}
