using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Repositories.Extensions;

namespace PickleBallBooking.Repositories.Configurations;

public class FieldConfiguration : IEntityTypeConfiguration<Field>
{
    public void Configure(EntityTypeBuilder<Field> builder)
    {
        builder.ToTable("Fields");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.FieldTypeId)
            .IsRequired();

        builder.Property(x => x.PricePerHour)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Area)
            .HasPrecision(10, 2);

        builder.Property(x => x.BluePrintImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Latitude)
            .HasPrecision(10, 8);

        builder.Property(x => x.Longitude)
            .HasPrecision(11, 8);

        builder.Property(x => x.MapUrl)
            .HasMaxLength(500);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.District)
            .HasMaxLength(100);

        builder.ConfigureAuditableEntity();

        // Relationships
        builder.HasOne(x => x.FieldType)
            .WithMany(ft => ft.Fields)
            .HasForeignKey(x => x.FieldTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Images)
            .WithOne(fi => fi.Field)
            .HasForeignKey(fi => fi.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Bookings)
            .WithOne(b => b.Field)
            .HasForeignKey(b => b.FieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Pricing)
            .WithOne(p => p.Field)
            .HasForeignKey(p => p.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.City);
        builder.HasIndex(x => x.District);
    }
}
