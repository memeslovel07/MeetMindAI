using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Persistence.Persistence.Extensions;

namespace MeetMindAI.Persistence.Persistence.Configurations;

/// <summary>
/// Configures the persistence mapping for the <see cref="RefreshToken"/> entity.
/// </summary>
public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("RefreshToken");

        builder.ConfigureBaseEntity();
        builder.ConfigureAuditableEntity();

        builder.Property(x => x.Token)
            .IsRequired();

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.RevokedAtUtc);

        builder.Property(x => x.RevocationReason)
            .HasConversion<int>();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReplacedByRefreshToken)
            .WithMany()
            .HasForeignKey(x => x.ReplacedByRefreshTokenId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
