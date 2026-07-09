using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MeetMindAI.Domain.Entities.Users;



using MeetMindAI.Domain.Constants;

using MeetMindAI.Persistence.Persistence.Extensions;

namespace MeetMindAI.Persistence.Persistence.Configurations;

/// <summary>
/// Configures the persistence mapping for the <see cref="User"/> entity.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("User");

        builder.ConfigureBaseEntity();
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Email)
            .HasMaxLength(ValidationConstants.EmailMaxLength)
            .IsRequired();

        builder.Property(x => x.NormalizedEmail)
            .HasMaxLength(ValidationConstants.EmailMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.NormalizedEmail)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(ValidationConstants.PasswordHashMaxLength)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(ValidationConstants.FirstNameMaxLength)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(ValidationConstants.LastNameMaxLength)
            .IsRequired();

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(ValidationConstants.AvatarUrlMaxLength);

        builder.Property(x => x.EmailConfirmed)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.LastLoginAtUtc);
    }
}
