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

    /// <summary>
    /// Marks the entity as deleted.
    /// </summary>
    protected void MarkAsDeleted(
        Guid? deletedBy,
        DateTime deletedAtUtc)
    {
        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAtUtc = deletedAtUtc;
    }

    /// <summary>
    /// Restores a previously deleted entity.
    /// </summary>
    protected void Restore()
    {
        IsDeleted = false;
        DeletedBy = null;
        DeletedAtUtc = null;
    }
}
