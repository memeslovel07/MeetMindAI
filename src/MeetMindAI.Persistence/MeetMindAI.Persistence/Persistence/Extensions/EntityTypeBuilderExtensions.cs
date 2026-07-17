using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeetMindAI.Domain.Common;

using Microsoft.EntityFrameworkCore.Metadata.Builders;



namespace MeetMindAI.Persistence.Persistence.Extensions;

/// <summary>
/// Provides common Entity Framework Core configuration extensions.
/// </summary>
public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Configures the common properties for all entities.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type.
    /// </typeparam>
    /// <param name="builder">
    /// The entity type builder.
    /// </param>
    public static void ConfigureBaseEntity<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(x => x.Id);

        builder.Ignore(x => x.DomainEvents);
    }

    /// <summary>
    /// Configures common audit properties.
    /// </summary>
    public static void ConfigureAuditableEntity<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedBy);

        builder.Property(x => x.UpdatedAtUtc);

        builder.Property(x => x.UpdatedBy);
    }

    /// <summary>
    /// Configures common soft delete properties.
    /// </summary>
    public static void ConfigureSoftDeletableEntity<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : SoftDeletableEntity
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.DeletedAtUtc);

        builder.Property(x => x.DeletedBy);
    }
}
