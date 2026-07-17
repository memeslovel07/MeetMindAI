using MeetMindAI.Domain.Constants;
using MeetMindAI.Domain.Entities.Meetings;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetMindAI.Persistence.Persistence.Configurations;

/// <summary>
/// Configures the Meeting entity.
/// </summary>
public sealed class MeetingConfiguration
    : IEntityTypeConfiguration<Meeting>
{
    public void Configure(
        EntityTypeBuilder<Meeting> builder)
    {
        builder.ToTable("Meeting");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(
                ValidationConstants.MeetingTitleMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(
                ValidationConstants.MeetingDescriptionMaxLength);

        builder.Property(x => x.DurationMinutes)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.ScheduledAtUtc);

        builder.HasOne(x => x.Organizer)
            .WithMany()
            .HasForeignKey(x => x.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrganizerId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.ScheduledAtUtc);
    }
}
