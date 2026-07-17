namespace MeetMindAI.Domain.Common;

/// <summary>
/// Defines soft deletion support for an entity.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity has been soft deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the entity was deleted.
    /// </summary>
    DateTime? DeletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user or system that deleted the entity.
    /// </summary>
    Guid? DeletedBy { get; set; }
}
