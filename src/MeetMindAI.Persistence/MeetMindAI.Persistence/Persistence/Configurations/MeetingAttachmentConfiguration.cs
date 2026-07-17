using MeetMindAI.Domain.Entities.Meetings;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetMindAI.Persistence.Configurations;

public sealed class MeetingAttachmentConfiguration : IEntityTypeConfiguration<MeetingAttachment>
{
    public void Configure(EntityTypeBuilder<MeetingAttachment> builder)
    {
        builder.ToTable("MeetingAttachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.StoredFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Extension)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.StorageKey)
      .HasMaxLength(500)
      .IsRequired();

        builder.Property(x => x.SizeInBytes)
            .IsRequired();

        builder.Property(x => x.AttachmentType)
            .HasConversion<int>()
            .IsRequired();

        

        builder.HasIndex(x => x.MeetingId);

        builder.HasOne(x => x.Meeting)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
