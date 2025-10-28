using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PickleBallBooking.Domain.Interfaces.Entities;

namespace PickleBallBooking.Repositories.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ConfigureAuditableEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IBaseAuditableEntity
    {
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.DeletedAt)
            .IsRequired(false);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Index for performance on common queries
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.CreatedAt);

        return builder;
    }
}
