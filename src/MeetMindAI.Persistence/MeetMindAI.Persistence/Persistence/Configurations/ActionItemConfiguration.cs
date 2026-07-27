using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeetMindAI.Domain.Entities.Users;
using MeetMindAI.Domain.Entities.Meetings;

namespace MeetMindAI.Persistence.Persistence.Configurations;

public sealed class ActionItemConfiguration
    : IEntityTypeConfiguration<ActionItem>
{
    public void Configure(
        EntityTypeBuilder<ActionItem> builder)
    {
        builder.ToTable("ActionItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(ActionItem.MaxTitleLength);

        builder.Property(x => x.Description)
            .HasMaxLength(ActionItem.MaxDescriptionLength);

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.DueDate);

        builder.Property(x => x.CompletedAt);

        builder.HasOne(x => x.Meeting)
            .WithMany()
            .HasForeignKey(x => x.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AssignedUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.MeetingId);

        builder.HasIndex(x => x.AssignedUserId);

        builder.HasIndex(x => new
        {
            x.MeetingId,
            x.Status
        });

        builder.Property(x => x.Source)
    .IsRequired();
    }
}
