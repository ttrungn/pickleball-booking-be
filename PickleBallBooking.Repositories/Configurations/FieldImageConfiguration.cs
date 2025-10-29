using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickleBallBooking.Domain.Entities;

namespace PickleBallBooking.Repositories.Configurations;

public class FieldImageConfiguration : IEntityTypeConfiguration<FieldImage>
{
    public void Configure(EntityTypeBuilder<FieldImage> builder)
    {
        builder.ToTable("FieldImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.FieldId)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(x => x.Field)
            .WithMany(f => f.Images)
            .HasForeignKey(x => x.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.FieldId);
    }
}
