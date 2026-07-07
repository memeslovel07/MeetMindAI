namespace MeetMindAI.Domain.Common;

/// <summary>
/// Represents an entity that supports soft deletion.
/// </summary>
public abstract class SoftDeletableEntity
    : AuditableEntity, ISoftDeletable
{
    /// <inheritdoc/>
    public bool IsDeleted { get; set; }

    /// <inheritdoc/>
    public DateTime? DeletedAtUtc { get; set; }

    /// <inheritdoc/>
    public Guid? DeletedBy { get; set; }
}
