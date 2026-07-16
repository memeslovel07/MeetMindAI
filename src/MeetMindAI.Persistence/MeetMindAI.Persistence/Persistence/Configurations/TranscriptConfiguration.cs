using MeetMindAI.Domain.Entities.Meetings;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetMindAI.Persistence.Persistence.Configurations;

public sealed class TranscriptConfiguration : IEntityTypeConfiguration<Transcript>
{
    public void Configure(EntityTypeBuilder<Transcript> builder)
    {
        builder.ToTable("Transcripts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.Language)
            .HasMaxLength(20);

        builder.HasOne(x => x.Meeting)
            .WithOne(x => x.Transcript)
            .HasForeignKey<Transcript>(x => x.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MeetingId)
            .IsUnique();
    }
}
