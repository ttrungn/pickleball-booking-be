using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Repositories.Extensions;

namespace PickleBallBooking.Repositories.Configurations;

public class FieldTypeConfiguration : IEntityTypeConfiguration<FieldType>
{
    public void Configure(EntityTypeBuilder<FieldType> builder)
    {
        builder.ToTable("FieldTypes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.ConfigureAuditableEntity();

        // Relationships
        builder.HasMany(x => x.Fields)
            .WithOne(f => f.FieldType)
            .HasForeignKey(f => f.FieldTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
