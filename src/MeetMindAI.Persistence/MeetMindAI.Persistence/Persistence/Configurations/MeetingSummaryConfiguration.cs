using MeetMindAI.Domain.Entities.Meetings;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetMindAI.Persistence.Configurations;

public sealed class MeetingSummaryConfiguration
    : IEntityTypeConfiguration<MeetingSummary>
{
    public void Configure(EntityTypeBuilder<MeetingSummary> builder)
    {
        builder.ToTable("MeetingSummaries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Summary)
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Model)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PromptVersion)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.GeneratedAtUtc)
            .IsRequired();

        builder.Property(x => x.IsRegenerated)
            .IsRequired();

        builder.HasOne(x => x.Meeting)
            .WithOne()
            .HasForeignKey<MeetingSummary>(x => x.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MeetingId)
            .IsUnique();
    }
}
